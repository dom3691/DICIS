# Visual Studio Roslyn Service Error Fix

## Problem
Getting error:
```
System.InvalidOperationException : Unexpected null - file BrokeredServiceConnection.cs line 92
```

This is a **Visual Studio IDE issue**, not your code. It's related to the Roslyn compiler service crashing.

## Quick Fixes (Try in Order)

### Fix 1: Restart Visual Studio
1. **Close Visual Studio completely**
2. **Reopen Visual Studio**
3. **Reopen your solution**

This fixes the issue 90% of the time.

### Fix 2: Clear Visual Studio Cache
1. **Close Visual Studio**
2. **Delete these folders**:
   - `%LOCALAPPDATA%\Microsoft\VisualStudio\17.0\ComponentModelCache`
   - `%LOCALAPPDATA%\Microsoft\VisualStudio\17.0\Extensions`
   - `%TEMP%\VSFeedbackIntelliCodeLogs`
3. **Restart Visual Studio**

### Fix 3: Reset Visual Studio Settings
1. **Close Visual Studio**
2. **Run Visual Studio Developer Command Prompt as Administrator**
3. **Run**:
   ```
   devenv /ResetSettings
   ```

### Fix 4: Repair Visual Studio
1. **Open Visual Studio Installer**
2. **Click "Modify" on your Visual Studio installation**
3. **Click "Repair"**
4. **Wait for repair to complete**
5. **Restart Visual Studio**

### Fix 5: Update Visual Studio
1. **Open Visual Studio Installer**
2. **Check for updates**
3. **Install any available updates**
4. **Restart Visual Studio**

## Alternative: Use Command Line

If Visual Studio keeps having issues, you can still work with the project:

### Build from Command Line:
```bash
cd "C:\Users\USER\Desktop\INDIGENE CERT"
dotnet build
```

### Run API from Command Line:
```bash
cd src/DICIS.API
dotnet run
```

### Run Blazor from Command Line:
```bash
cd src/DICIS.Blazor
dotnet run
```

## Verify Your Code is Fine

Your code is likely fine. To verify:

1. **Build from command line** (see above)
2. **If build succeeds**, the error is just Visual Studio's IDE
3. **Your application will still run correctly**

## Most Common Solution

**Simply restart Visual Studio** - this fixes it most of the time!

The error doesn't affect your actual code or application - it's just the IDE's language service having issues.
