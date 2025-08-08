# SystemOperations.cs Change History

## Guard Windows EventLog access (2025-07-26)
- Wrapped Event Log retrieval in `OperatingSystem.IsWindows()` check to satisfy CA1416.
