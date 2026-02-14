# How to Get Your OTP

## Current Setup (Development Mode)

In development mode, the OTP is displayed in **two places**:

### Method 1: On the Login Page (Easiest)
1. Enter your NIN (any 11-digit number, e.g., `12345678901`)
2. Click "Verify NIN"
3. **The OTP will be displayed in a blue info message** on the login page
4. Look for: `"OTP sent! (Development mode - OTP: XXXXXX)"`
5. Enter that 6-digit OTP in the OTP field

### Method 2: API Console Output
1. Enter your NIN and click "Verify NIN"
2. Check the **terminal/console where the API is running**
3. You'll see: `OTP for NIN 12345678901: 123456`
4. Use that 6-digit number as your OTP

## Example Flow

1. **Go to Login Page**: `https://localhost:7001/login`

2. **Enter NIN**: 
   - Any 11-digit number (e.g., `12345678901`)
   - Click "Verify NIN"

3. **Get OTP**:
   - **Option A**: Check the blue message on the page showing the OTP
   - **Option B**: Check the API console output

4. **Enter OTP**:
   - Enter the 6-digit OTP shown
   - Click "Verify OTP"

5. **You're logged in!**

## Important Notes

- **OTP expires in 5 minutes**
- **OTP is 6 digits** (e.g., `123456`, `987654`)
- **Each NIN gets a new OTP** when you verify the NIN again
- **In production**, OTPs would be sent via SMS/Email (not displayed)

## Troubleshooting

### "OTP not found or expired"
- The OTP expired (5 minutes)
- You entered a different NIN
- **Solution**: Verify your NIN again to get a new OTP

### "Invalid OTP"
- You entered the wrong OTP
- **Solution**: Check the OTP again (from page message or console)

### Can't see OTP on page
- Check the API console output instead
- Or verify your NIN again

## Testing with Sample NINs

You can use any 11-digit number as NIN:
- `12345678901`
- `98765432109`
- `11111111111`
- etc.

All will work in development mode!
