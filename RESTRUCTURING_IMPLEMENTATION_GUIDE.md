# 🚀 ML Creator Restructuring - Quick Start Guide

## ⚡ Immediate Actions (Today)

### **1. Create Domain Structure**
```bash
# Create the new folder hierarchy
mkdir -p Assets/Domains/{Characters,Perception,Networking,UI,World}
mkdir -p Assets/{Core,Shared,Tools,Integrations}

# Create domain subfolders
for domain in Characters Perception Networking UI World; do
  mkdir -p Assets/Domains/$domain/{Runtime,Editor,Tests}
  mkdir -p Assets/Domains/$domain/Runtime/{Components,Systems,Data,Events,Services}
  mkdir -p Assets/Domains/$domain/Editor/{Components,Inspectors,Windows,Wizards}
done
```

### **2. Assembly Definition Files**
Create `.asmdef` files for each domain:

**Assets/Domains/Characters/Runtime/GameCreator.Characters.Runtime.asmdef**
```json
{
  "name": "GameCreator.Characters.Runtime",
  "rootNamespace": "GameCreator.Characters",
  "references": ["GameCreator.Core.Runtime"],
  "includePlatforms": ["Editor", "Windows64", "macOS"],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": []
}
```

### **3. First Migration - Character Domain**

#### **Step 3.1: Move Core Files**
```bash
# Move character components
mv Assets/Plugins/GameCreator/Packages/Core/Runtime/Characters/Components/* \
   Assets/Domains/Characters/Runtime/Components/

# Move character systems
mv Assets/Plugins/GameCreator/Packages/Core/Runtime/Characters/Systems/* \
   Assets/Domains/Characters/Runtime/Systems/
```

#### **Step 3.2: Update Namespaces**
**BEFORE:**
```csharp
namespace GameCreator.Runtime.Characters
{
    public class Character : MonoBehaviour
    {
        // CharacterMovementController (27 chars - too long!)
    }
}
```

**AFTER:**
```csharp
namespace GameCreator.Characters
{
    public class CharacterMotor : MonoBehaviour  // 15 chars - efficient!
    {
        // Clean, focused implementation
    }
}
```

#### **Step 3.3: Update References**
Use find-and-replace tools to update all namespace references:

```bash
# Update using directives
find Assets -name "*.cs" -exec sed -i 's/GameCreator\.Runtime\.Characters/GameCreator.Characters/g' {} \;

# Update fully qualified names
find Assets -name "*.cs" -exec sed -i 's/GameCreator\.Runtime\.Characters\./GameCreator.Characters./g' {} \;
```

## 🔄 Namespace Migration Patterns

### **Pattern 1: Runtime → Domain**
```
OLD: GameCreator.Runtime.Characters.Systems.Units.Driver.NavMeshDriver
NEW: GameCreator.Characters.Units.Driver.NavMeshDriver
```

### **Pattern 2: Class Renaming**
```
OLD: CharacterMovementController (27 chars)
NEW: CharacterMotor (14 chars) - 48% reduction!
```

### **Pattern 3: File Organization**
```
OLD: Assets/Plugins/GameCreator/Packages/Core/Runtime/Characters/Systems/Units/Driver/NavMeshDriver.cs
NEW: Assets/Domains/Characters/Runtime/Systems/Driver/NavMeshDriver.cs
```

## 🎯 Efficiency Metrics to Track

### **Naming Efficiency**
- **Target**: Max 25 characters per class name
- **Monitor**: Average name length reduction (target: 40%+)

### **Navigation Speed**
- **Target**: <30 seconds to find any component
- **Measure**: Developer surveys post-migration

### **Build Performance**
- **Target**: <5 minute full rebuilds
- **Monitor**: Build time before/after migration

## 🧪 Validation Checklist

### **Phase 1 Validation**
- [ ] All domains have proper folder structure
- [ ] Assembly definitions created and referenced
- [ ] No compilation errors in moved files

### **Phase 2 Validation**
- [ ] All namespace references updated
- [ ] All class names follow new conventions
- [ ] Tests pass for migrated domains

### **Phase 3 Validation**
- [ ] Unity scenes load without errors
- [ ] Prefabs maintain functionality
- [ ] Runtime behavior unchanged

## 🆘 Troubleshooting

### **Common Issues**

#### **"Type not found" Errors**
```csharp
// Check namespace references
using GameCreator.Characters; // Correct
// using GameCreator.Runtime.Characters; // Wrong
```

#### **Assembly Reference Errors**
```json
// Ensure proper dependencies in .asmdef
{
  "references": [
    "GameCreator.Core.Runtime",
    "UnityEngine.CoreModule"
  ]
}
```

#### **File Path Issues**
```bash
# Verify file locations match namespaces
find Assets/Domains -name "*.cs" | head -10
# Should show: Assets/Domains/Characters/Runtime/Components/CharacterMotor.cs
```

## 📞 Support Resources

### **Quick Reference**
- **Proposal**: See `PROJECT_RESTRUCTURING_PROPOSAL.md`
- **Migration Scripts**: Check `scripts/` folder
- **Naming Standards**: See proposal section 3.2

### **Help Commands**
```bash
# Check current structure
find Assets -type d -name "*omponent*" | sort

# Validate namespaces
grep -r "^namespace " Assets/Domains/ | wc -l

# Check for long names
find Assets/Domains -name "*.cs" -exec basename {} \; | awk 'length > 25 {print length, $0}' | sort -nr
```

## 🎉 Success Indicators

### **Immediate Success**
- ✅ Project compiles without errors
- ✅ Unity Editor opens scenes correctly
- ✅ No console errors in play mode

### **Team Success**
- 👥 Developers can find files quickly
- 👥 New team members understand structure
- 👥 Code reviews focus on logic, not organization

### **Project Success**
- 📈 Faster feature development
- 📈 Fewer architectural debates
- 📈 Higher code quality metrics

---

**Remember**: This is a significant architectural improvement that will pay dividends throughout the project's lifetime. The initial investment in restructuring will be repaid many times over in developer productivity and code maintainability.

Start small, validate each step, and celebrate progress! 🚀
