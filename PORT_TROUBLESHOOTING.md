# Port Troubleshooting Guide

## Issue: Port Already in Use

If you get an error like:
```
Failed to bind to address http://127.0.0.1:5000: address already in use
```

This means another process is already using that port.

## Quick Solutions

### Option 1: Kill the Process (Recommended)

1. **Find the process using the port:**
   ```powershell
   netstat -ano | findstr :5000
   ```
   Note the PID (Process ID) from the output

2. **Kill the process:**
   ```powershell
   taskkill /F /PID <PID>
   ```
   Replace `<PID>` with the actual process ID

### Option 2: Change the Port

If you want to use a different port, edit `src/DICIS.API/Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5001"  // Changed from 5000
    },
    "https": {
      "applicationUrl": "https://localhost:7001;http://localhost:5001"
    }
  }
}
```

**Important:** If you change the API port, also update the Blazor app's API URL in `src/DICIS.Blazor/Program.cs`:

```csharp
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("https://localhost:7001") // Match your API port
});
```

### Option 3: Use Different Ports for Each Project

**API (Backend):**
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7000`

**Blazor (Frontend):**
- HTTP: `http://localhost:5001`
- HTTPS: `https://localhost:7001`

This is the default configuration and should work if no other apps are using these ports.

## Common Causes

1. **Previous instance still running**: The API didn't shut down properly
2. **Another application using the port**: Another .NET app or service
3. **Visual Studio debugging**: Multiple debug sessions running

## Prevention

1. **Always stop the application properly** before starting a new instance
2. **Use Ctrl+C** in the terminal to stop the app gracefully
3. **Close all terminal windows** before starting a new instance
4. **Check Task Manager** if ports seem stuck

## Verify Ports Are Free

```powershell
# Check if ports are in use
netstat -ano | findstr ":5000"
netstat -ano | findstr ":7000"
netstat -ano | findstr ":5001"
netstat -ano | findstr ":7001"
```

If no output, the ports are free!

## Alternative: Use Random Ports

You can let .NET choose available ports automatically by removing the `applicationUrl` from `launchSettings.json`. However, you'll need to check the console output to see which port was assigned.
