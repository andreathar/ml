using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Central registry for all active NetworkCharacters.
    /// Provides efficient lookup by ClientId and caches the local player reference.
    ///
    /// Host vs Client:
    /// - Host (Server + Client): LocalClientId == 0, IsHost == true
    /// - Client (Pure client): LocalClientId > 0, IsHost == false
    /// - Dedicated Server: IsServer && !IsClient, no local player
    /// </summary>
    public static class NetworkCharacterRegistry
    {
        // MEMBERS: -------------------------------------------------------------------------------

        private static readonly List<NetworkCharacter> s_Characters = new();
        private static readonly Dictionary<ulong, NetworkCharacter> s_CharactersByClientId = new();
        private static NetworkCharacter s_LocalPlayerCache;
        private static bool s_LocalPlayerCacheDirty = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// All registered NetworkCharacters (spawned and active).
        /// </summary>
        public static IReadOnlyList<NetworkCharacter> All => s_Characters;

        /// <summary>
        /// Number of registered NetworkCharacters.
        /// </summary>
        public static int Count => s_Characters.Count;

        /// <summary>
        /// The local player's NetworkCharacter.
        /// - For Host: The character owned by ClientId 0
        /// - For Client: The character owned by this client's LocalClientId
        /// - For Dedicated Server: null (no local player)
        /// - For Single-player (no NetworkManager): null
        /// </summary>
        public static NetworkCharacter LocalPlayer
        {
            get
            {
                if (s_LocalPlayerCacheDirty)
                {
                    s_LocalPlayerCache = FindLocalPlayer();
                    s_LocalPlayerCacheDirty = false;
                }
                return s_LocalPlayerCache;
            }
        }

        /// <summary>
        /// True if running as Host (Server + Client combined).
        /// Host has LocalClientId == 0 and owns the first player character.
        /// </summary>
        public static bool IsHost =>
            NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;

        /// <summary>
        /// True if running as pure Client (not Host).
        /// Client has LocalClientId > 0.
        /// </summary>
        public static bool IsClient =>
            NetworkManager.Singleton != null
            && NetworkManager.Singleton.IsClient
            && !NetworkManager.Singleton.IsHost;

        /// <summary>
        /// True if running as Dedicated Server (no local client).
        /// Dedicated servers have no "local player".
        /// </summary>
        public static bool IsDedicatedServer =>
            NetworkManager.Singleton != null
            && NetworkManager.Singleton.IsServer
            && !NetworkManager.Singleton.IsClient;

        /// <summary>
        /// True if any network session is active.
        /// </summary>
        public static bool IsNetworkActive =>
            NetworkManager.Singleton != null
            && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);

        /// <summary>
        /// The local client's ID.
        /// Returns 0 for Host, 1+ for Clients, ulong.MaxValue if not connected.
        /// </summary>
        public static ulong LocalClientId =>
            NetworkManager.Singleton != null
                ? NetworkManager.Singleton.LocalClientId
                : ulong.MaxValue;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Register a NetworkCharacter with the registry.
        /// Called automatically by NetworkCharacter on network spawn.
        /// </summary>
        public static void Register(NetworkCharacter character)
        {
            if (character == null) return;
            if (s_Characters.Contains(character)) return;

            s_Characters.Add(character);

            var networkObject = character.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                ulong ownerId = networkObject.OwnerClientId;
                s_CharactersByClientId[ownerId] = character;

                Debug.Log(
                    $"[NetworkCharacterRegistry] Registered {character.name} "
                        + $"(ClientId: {ownerId}, IsLocalOwner: {networkObject.IsOwner})"
                );
            }

            InvalidateLocalPlayerCache();
        }

        /// <summary>
        /// Unregister a NetworkCharacter from the registry.
        /// Called automatically by NetworkCharacter on network despawn or destroy.
        /// </summary>
        public static void Unregister(NetworkCharacter character)
        {
            if (character == null) return;

            bool removed = s_Characters.Remove(character);

            // Remove from ClientId dictionary
            var keysToRemove = s_CharactersByClientId
                .Where(kvp => kvp.Value == character)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                s_CharactersByClientId.Remove(key);
            }

            if (removed)
            {
                Debug.Log($"[NetworkCharacterRegistry] Unregistered {character.name}");
            }

            InvalidateLocalPlayerCache();
        }

        /// <summary>
        /// Get a NetworkCharacter by its owner's ClientId.
        /// </summary>
        /// <param name="clientId">The ClientId of the owner (0 for Host, 1+ for Clients)</param>
        /// <returns>The NetworkCharacter owned by that client, or null if not found</returns>
        public static NetworkCharacter GetByClientId(ulong clientId)
        {
            return s_CharactersByClientId.TryGetValue(clientId, out var character)
                ? character
                : null;
        }

        /// <summary>
        /// Get the position of a NetworkCharacter by its owner's ClientId.
        /// </summary>
        public static Vector3 GetPositionByClientId(ulong clientId)
        {
            var character = GetByClientId(clientId);
            return character != null ? character.transform.position : Vector3.zero;
        }

        /// <summary>
        /// Get the rotation of a NetworkCharacter by its owner's ClientId.
        /// </summary>
        public static Quaternion GetRotationByClientId(ulong clientId)
        {
            var character = GetByClientId(clientId);
            return character != null ? character.transform.rotation : Quaternion.identity;
        }

        /// <summary>
        /// Invalidate the local player cache.
        /// Call this when ownership changes or network state changes.
        /// </summary>
        public static void InvalidateLocalPlayerCache()
        {
            s_LocalPlayerCacheDirty = true;
        }

        /// <summary>
        /// Update the ClientId mapping for a character (e.g., after ownership transfer).
        /// </summary>
        public static void UpdateClientIdMapping(NetworkCharacter character, ulong newClientId)
        {
            if (character == null) return;

            // Remove old mapping
            var oldKeys = s_CharactersByClientId
                .Where(kvp => kvp.Value == character)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in oldKeys)
            {
                s_CharactersByClientId.Remove(key);
            }

            // Add new mapping
            s_CharactersByClientId[newClientId] = character;

            InvalidateLocalPlayerCache();
        }

        /// <summary>
        /// Get all NetworkCharacters except the local player.
        /// Useful for "find all other players" scenarios.
        /// </summary>
        public static IEnumerable<NetworkCharacter> GetOtherPlayers()
        {
            var localPlayer = LocalPlayer;
            return s_Characters.Where(c => c != localPlayer);
        }

        /// <summary>
        /// Get the closest NetworkCharacter to a position (excluding the local player).
        /// </summary>
        public static NetworkCharacter GetClosestOtherPlayer(Vector3 position)
        {
            NetworkCharacter closest = null;
            float closestDistance = float.MaxValue;

            foreach (var character in GetOtherPlayers())
            {
                float distance = Vector3.Distance(position, character.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = character;
                }
            }

            return closest;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static NetworkCharacter FindLocalPlayer()
        {
            // No network manager = no network player
            if (NetworkManager.Singleton == null)
            {
                return null;
            }

            // Dedicated server has no local player
            if (IsDedicatedServer)
            {
                return null;
            }

            // Not connected yet
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            {
                return null;
            }

            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            return GetByClientId(localClientId);
        }

        // INITIALIZATION: ------------------------------------------------------------------------

        /// <summary>
        /// Reset static state on domain reload (Editor) or game restart.
        /// This prevents stale references between play sessions.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_Characters.Clear();
            s_CharactersByClientId.Clear();
            s_LocalPlayerCache = null;
            s_LocalPlayerCacheDirty = true;
        }
    }
}
