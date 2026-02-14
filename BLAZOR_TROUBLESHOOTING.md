# Blazor WebAssembly Error Troubleshooting

## "An unhandled error has occurred" - Debugging Steps

### Step 1: Check Browser Console

1. **Open Developer Tools:**
   - Press `F12` or `Ctrl+Shift+I`
   - Go to the **Console** tab

2. **Look for errors:**
   - Red error messages
   - Failed network requests
   - JavaScript exceptions

3. **Common errors to look for:**
   - `Failed to fetch` - API not running or CORS issue
   - `404 Not Found` - Missing files
   - `TypeError` - JavaScript errors
   - `NETSDK` errors - Build issues

### Step 2: Check Network Tab

1. **Open Network tab** in Developer Tools
2. **Reload the page** (F5)
3. **Check for failed requests:**
   - Red entries indicate failures
   - Check if `blazor.webassembly.js` loads (should be 200)
   - Check if `_framework` files load
   - Check API calls (if any)

### Step 3: Verify API is Running

The Blazor app tries to connect to the API at `https://localhost:7000`. Make sure:

1. **API is running:**
   ```bash
   cd src/DICIS.API
   dotnet run
   ```
   Should show: `Now listening on: https://localhost:7000`

2. **Check API is accessible:**
   - Open `https://localhost:7000/swagger` in browser
   - Should show Swagger UI

3. **CORS is configured:**
   - Check `src/DICIS.API/Program.cs`
   - Should allow `https://localhost:7001`

### Step 4: Clear Browser Cache

1. **Hard refresh:**
   - `Ctrl+Shift+R` (Windows/Linux)
   - `Cmd+Shift+R` (Mac)

2. **Clear cache:**
   - `Ctrl+Shift+Delete`
   - Select "Cached images and files"
   - Clear data

### Step 5: Rebuild the Application

```bash
# Clean and rebuild
cd src/DICIS.Blazor
dotnet clean
dotnet build
dotnet run
```

### Step 6: Check for Common Issues

#### Issue 1: API Not Running
**Symptom:** Console shows "Failed to fetch" or network errors
**Solution:** Start the API first, then the Blazor app

#### Issue 2: CORS Error
**Symptom:** Console shows CORS policy error
**Solution:** 
- Check API `Program.cs` CORS configuration
- Ensure Blazor URL is in allowed origins

#### Issue 3: Missing Files
**Symptom:** 404 errors for `_framework` files
**Solution:**
- Rebuild the project
- Check `bin/Debug/net8.0/wwwroot/_framework` exists

#### Issue 4: SSL Certificate Issues
**Symptom:** "NET::ERR_CERT_AUTHORITY_INVALID"
**Solution:**
- Click "Advanced" → "Proceed to localhost"
- Or use HTTP instead of HTTPS

### Step 7: Enable Detailed Error Messages

Add to `wwwroot/index.html` (already added):
```html
<script>
    window.addEventListener('error', function (e) {
        console.error('Global error:', e.error);
    });
    window.addEventListener('unhandledrejection', function (e) {
        console.error('Unhandled promise rejection:', e.reason);
    });
</script>
```

### Step 8: Check Application Logs

If running from terminal, check the console output for:
- Build errors
- Runtime exceptions
- Missing dependencies

### Common Fixes

#### Fix 1: Start API First
```bash
# Terminal 1 - Start API
cd src/DICIS.API
dotnet run

# Terminal 2 - Start Blazor (after API is running)
cd src/DICIS.Blazor
dotnet run
```

#### Fix 2: Use HTTP Instead of HTTPS
If SSL issues persist, update `launchSettings.json`:
```json
"applicationUrl": "http://localhost:5001"
```

And update `Program.cs`:
```csharp
BaseAddress = new Uri("http://localhost:5000")
```

#### Fix 3: Check Port Conflicts
```powershell
netstat -ano | findstr :7001
netstat -ano | findstr :5001
```

### Getting Help

If the error persists:
1. **Copy the exact error** from browser console
2. **Check the Network tab** for failed requests
3. **Verify both API and Blazor are running**
4. **Check the terminal output** for build/runtime errors

### Quick Checklist

- [ ] API is running on port 7000
- [ ] Blazor is running on port 7001
- [ ] Browser console shows no errors
- [ ] Network tab shows successful file loads
- [ ] CORS is configured correctly
- [ ] Browser cache is cleared
- [ ] Project is rebuilt
