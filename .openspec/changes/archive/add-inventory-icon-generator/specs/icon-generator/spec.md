## ADDED Requirements
### Requirement: Batch Icon Generation
The system SHALL provide a Unity Editor interface to generate icon assets from a text list of item names.

#### Scenario: Batch process
- **WHEN** the user provides a newline-separated list of item names
- **AND** clicks "Generate"
- **THEN** the system generates a unique image for each item using the configured AI service
- **AND** saves the images as PNG files in the specified output folder

### Requirement: Style Configuration
The system SHALL allow users to define a visual style prompt that is applied to all generated icons to ensure consistency.

#### Scenario: Style application
- **WHEN** the user sets Style to "Pixel Art"
- **AND** generates an icon for "Apple"
- **THEN** the resulting image matches the pixel art aesthetic

### Requirement: GameCreator Integration
The system SHALL provide an option to automatically create GameCreator Item assets and assign the generated icons.

#### Scenario: Item creation
- **WHEN** "Create Items" is checked
- **AND** generation completes
- **THEN** a new GameCreator Item ScriptableObject is created for each name
- **AND** the generated sprite is assigned to the Item's icon field

