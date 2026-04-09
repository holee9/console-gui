// Copyright (c) HnVue Medical. All rights reserved.
// Global suppressions for StyleCop warnings — HnVue.Update module.

// SA1101: We do not use 'this.' prefix for member access - project convention
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SA1101:PrefixLocalCallsWithThis", Justification = "Project convention: no 'this.' prefix")]

// SA1309: Field names with underscore prefix are allowed per .editorconfig naming convention
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Private fields use _camelCase per .editorconfig")]

// SA1312: Variable names with underscore prefix are allowed per .editorconfig naming convention
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SA1312:VariableNamesMustNotBeginWithUnderscore", Justification = "Private fields use _camelCase per .editorconfig")]

// SA1200: Using directives should be inside namespace - but our convention is outside
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SA1200:UsingDirectivesShouldBePlacedWithinNamespace", Justification = "Project convention: using statements outside namespace")]

// SA1600: Elements should be documented - suppress for internal/private only
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SA1600:ElementsMustBeDocumented", Justification = "Internal and private members do not require XML docs per .stylecop.json")]

// SA1413: Use trailing comma - not required by project style guide
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SA1413:UseTrailingCommas", Justification = "Trailing commas not required by project style")]
