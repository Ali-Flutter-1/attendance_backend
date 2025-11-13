# Flutter GPS Location Example

This guide shows you how to get the user's GPS coordinates (latitude and longitude) in Flutter when they click a button.

## Step 1: Add Dependencies

Add these packages to your `pubspec.yaml`:

```yaml
dependencies:
  flutter:
    sdk: flutter
  geolocator: ^10.1.0  # For getting GPS location
  permission_handler: ^11.0.0  # For requesting location permissions
  http: ^1.1.0  # For making API calls
  image_picker: ^1.0.4  # For taking pictures
```

Then run:
```bash
flutter pub get
```

## Step 2: Add Permissions

### Android (`android/app/src/main/AndroidManifest.xml`)

Add these permissions before the `<application>` tag:

```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

### iOS (`ios/Runner/Info.plist`)

Add these keys:

```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>We need your location to verify you are at the office for attendance.</string>
<key>NSLocationAlwaysUsageDescription</key>
<string>We need your location to verify you are at the office for attendance.</string>
<key>NSCameraUsageDescription</key>
<string>We need camera access to take your attendance photo.</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>We need photo library access to select your attendance photo.</string>
```

## Step 3: Complete Flutter Code Example

Here's a complete example of a check-in screen:

```dart
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:image_picker/image_picker.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class CheckInScreen extends StatefulWidget {
  final int userId;
  
  const CheckInScreen({Key? key, required this.userId}) : super(key: key);

  @override
  State<CheckInScreen> createState() => _CheckInScreenState();
}

class _CheckInScreenState extends State<CheckInScreen> {
  double? _latitude;
  double? _longitude;
  File? _image;
  bool _isLoading = false;
  String _statusMessage = '';
  final ImagePicker _picker = ImagePicker();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Check In'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Location Status
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  children: [
                    const Icon(Icons.location_on, size: 48, color: Colors.blue),
                    const SizedBox(height: 8),
                    Text(
                      _latitude != null && _longitude != null
                          ? 'Location: ${_latitude!.toStringAsFixed(6)}, ${_longitude!.toStringAsFixed(6)}'
                          : 'Location not obtained',
                      style: const TextStyle(fontSize: 14),
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 20),
            
            // Image Preview
            if (_image != null)
              Container(
                height: 200,
                width: 200,
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.grey),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Image.file(_image!, fit: BoxFit.cover),
              )
            else
              Container(
                height: 200,
                width: 200,
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.grey),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Icon(Icons.camera_alt, size: 48, color: Colors.grey),
              ),
            
            const SizedBox(height: 20),
            
            // Status Message
            if (_statusMessage.isNotEmpty)
              Padding(
                padding: const EdgeInsets.all(8.0),
                child: Text(
                  _statusMessage,
                  style: TextStyle(
                    color: _statusMessage.contains('success') 
                        ? Colors.green 
                        : Colors.red,
                    fontSize: 14,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
            
            const SizedBox(height: 20),
            
            // Buttons
            ElevatedButton.icon(
              onPressed: _isLoading ? null : _getCurrentLocation,
              icon: const Icon(Icons.location_on),
              label: const Text('Get My Location'),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
              ),
            ),
            
            const SizedBox(height: 10),
            
            ElevatedButton.icon(
              onPressed: _isLoading ? null : _takePicture,
              icon: const Icon(Icons.camera_alt),
              label: const Text('Take Picture'),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
              ),
            ),
            
            const SizedBox(height: 20),
            
            // Check In Button
            ElevatedButton(
              onPressed: (_latitude != null && _longitude != null && _image != null && !_isLoading)
                  ? _checkIn
                  : null,
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.green,
                padding: const EdgeInsets.symmetric(horizontal: 48, vertical: 16),
                disabledBackgroundColor: Colors.grey,
              ),
              child: _isLoading
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : const Text(
                      'Check In',
                      style: TextStyle(fontSize: 18, color: Colors.white),
                    ),
            ),
          ],
        ),
      ),
    );
  }

  // Get current GPS location
  Future<void> _getCurrentLocation() async {
    try {
      setState(() {
        _statusMessage = 'Getting location...';
        _isLoading = true;
      });

      // Check if location services are enabled
      bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) {
        setState(() {
          _statusMessage = 'Location services are disabled. Please enable them.';
          _isLoading = false;
        });
        return;
      }

      // Check location permission
      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) {
          setState(() {
            _statusMessage = 'Location permissions are denied.';
            _isLoading = false;
          });
          return;
        }
      }

      if (permission == LocationPermission.deniedForever) {
        setState(() {
          _statusMessage = 'Location permissions are permanently denied. Please enable in settings.';
          _isLoading = false;
        });
        return;
      }

      // Get current position
      Position position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
        timeLimit: const Duration(seconds: 10),
      );

      setState(() {
        _latitude = position.latitude;
        _longitude = position.longitude;
        _statusMessage = 'Location obtained successfully!';
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _statusMessage = 'Error getting location: $e';
        _isLoading = false;
      });
    }
  }

  // Take picture using camera
  Future<void> _takePicture() async {
    try {
      final XFile? image = await _picker.pickImage(
        source: ImageSource.camera,
        imageQuality: 85,
        maxWidth: 1024,
        maxHeight: 1024,
      );

      if (image != null) {
        setState(() {
          _image = File(image.path);
          _statusMessage = 'Picture taken successfully!';
        });
      }
    } catch (e) {
      setState(() {
        _statusMessage = 'Error taking picture: $e';
      });
    }
  }

  // Check in to the API
  Future<void> _checkIn() async {
    if (_latitude == null || _longitude == null || _image == null) {
      setState(() {
        _statusMessage = 'Please get location and take a picture first.';
      });
      return;
    }

    try {
      setState(() {
        _isLoading = true;
        _statusMessage = 'Checking in...';
      });

      // Create multipart request
      var request = http.MultipartRequest(
        'POST',
        Uri.parse('https://localhost:7225/api/attendance/checkin'), // Replace with your API URL
      );

      // Add form fields
      request.fields['userId'] = widget.userId.toString();
      request.fields['latitude'] = _latitude!.toString();
      request.fields['longitude'] = _longitude!.toString();

      // Add image file
      request.files.add(
        await http.MultipartFile.fromPath(
          'picture',
          _image!.path,
        ),
      );

      // Send request
      var streamedResponse = await request.send();
      var response = await http.Response.fromStream(streamedResponse);

      // Parse response
      var responseData = json.decode(response.body);

      setState(() {
        _isLoading = false;
        if (responseData['success'] == true) {
          _statusMessage = 'Check-in successful! ✅';
          // Reset form after successful check-in
          Future.delayed(const Duration(seconds: 2), () {
            setState(() {
              _latitude = null;
              _longitude = null;
              _image = null;
              _statusMessage = '';
            });
          });
        } else {
          _statusMessage = 'Error: ${responseData['message']}';
        }
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _statusMessage = 'Error checking in: $e';
      });
    }
  }
}
```

## Step 4: Usage Example

```dart
// In your main app or navigation
Navigator.push(
  context,
  MaterialPageRoute(
    builder: (context) => CheckInScreen(userId: 1), // Pass actual user ID
  ),
);
```

## Step 5: Check Out Example

For check-out, use the same code but change the endpoint:

```dart
// Change this line in _checkIn method:
Uri.parse('https://localhost:7225/api/attendance/checkout'),
```

## Important Notes

1. **Replace API URL**: Change `https://localhost:7225` to your actual API URL
   - For Android emulator: `http://10.0.2.2:5201` (HTTP) or `https://10.0.2.2:7225` (HTTPS)
   - For iOS simulator: `http://localhost:5201` or `https://localhost:7225`
   - For physical device: Use your computer's IP address, e.g., `http://192.168.1.100:5201`

2. **HTTPS Certificate**: If using HTTPS with self-signed certificate, you may need to disable certificate validation in development:
   ```dart
   // Add this import
   import 'package:http/io_client.dart' as io;
   import 'dart:io';
   
   // Create HTTP client that ignores SSL errors (development only!)
   final httpClient = HttpClient()
     ..badCertificateCallback = (X509Certificate cert, String host, int port) => true;
   final client = io.IOClient(httpClient);
   ```

3. **Location Accuracy**: The code uses `LocationAccuracy.high` for best accuracy. You can change this if needed.

4. **Error Handling**: Make sure to handle all error cases properly in production.

## Testing

1. Run the app on a physical device (GPS doesn't work well on emulators)
2. Click "Get My Location" button
3. Grant location permission when prompted
4. Click "Take Picture" button
5. Grant camera permission when prompted
6. Click "Check In" button
7. Verify the API response

## Troubleshooting

- **Location not working**: Make sure GPS is enabled on the device
- **Permission denied**: Check that permissions are added to AndroidManifest.xml and Info.plist
- **API connection failed**: Check your API URL and make sure the API is running
- **SSL errors**: Use HTTP in development or configure SSL properly

