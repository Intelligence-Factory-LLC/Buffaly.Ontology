# Method Resolution Strategy

`ReflectionUtil.GetMethod` selects the most suitable .NET method when several overloads
match a name and parameter count. The algorithm is:

1. Enumerate all candidate methods with the same name and arity.
2. Try to locate an exact match using the provided argument types.
3. If that fails, normalise wrapper types (e.g., `IntWrapper` → `int`) and search again.
4. If no direct match is found, each candidate is scored:
   - `0` for parameters whose types exactly match the argument types.
   - `1` when wrappers map to their primitive counterparts.
   - `2` when the candidate parameter type is assignable from the argument type.
   - Incompatible parameters discard the candidate.
5. The candidate with the lowest score is returned, or `null` if none are compatible.

This approach ensures that strongly typed overloads are chosen ahead of more
general versions such as those accepting `object` parameters.
