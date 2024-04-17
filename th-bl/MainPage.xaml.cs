using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace th_bl
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly IAdapter _bluetoothAdapter;                            // Class for the Bluetooth adapter
        private readonly List<IDevice> _gattDevices = new List<IDevice>();

        public MainPage()
        {
            InitializeComponent();

            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;               // Point _bluetoothAdapter to the current adapter on the phone
            _bluetoothAdapter.DeviceDiscovered += (sender, foundBleDevice) =>   // When a BLE Device is found, run the small function below to add it to our list
            {
                if (foundBleDevice.Device != null && !string.IsNullOrEmpty(foundBleDevice.Device.Name))
                    _gattDevices.Add(foundBleDevice.Device);
            };
        }

        private async Task<bool> PermissionsGrantedAsync()      // Function to make sure that all the appropriate approvals are in place
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }

        private async void ScanButton_Clicked(object sender, EventArgs e)           // Function that is called when the scanButton is pressed
        {
            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = false);        // Swith the Isbusy Indicator on
            foundBleDevicesListView.ItemsSource = null;                                                     // Empty the list of found BLE devices (in the GUI)

            if (!await PermissionsGrantedAsync())                                                           // Make sure there is permission to use Bluetooth
            {
                await DisplayAlert("Permission required", "Application needs location permission", "OK");
                IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);
                return;
            }

            _gattDevices.Clear();                                                                           // Also clear the _gattDevices list

            if (!_bluetoothAdapter.IsScanning)                                                              // Make sure that the Bluetooth adapter is scanning for devices
            {
                await _bluetoothAdapter.StartScanningForDevicesAsync();
            }

            foreach (var device in _bluetoothAdapter.ConnectedDevices)                                      // Make sure BLE devices are added to the _gattDevices list
                _gattDevices.Add(device);

            foundBleDevicesListView.ItemsSource = _gattDevices.ToArray();                                   // Write found BLE devices to GUI
            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);         // Switch off the busy indicator
        }

        private async void FoundBluetoothDevicesListView_ItemTapped(object sender, ItemTappedEventArgs e)   // Function that is run whenever a detected BLE device is selected
        {
            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = false);        // Switch on IsBusy indicator
            IDevice selectedItem = e.Item as IDevice;                                                       // The item selected is an IDevice (detected BLE device). Therefore we have to cast the selected item to an IDevice

            if (selectedItem.State == DeviceState.Connected)                                                // Check first if we are already connected to the BLE Device 
            {
                await OnPrint(selectedItem);                                    // Navigate to the Services Page to show the services of the selected BLE Device
            }
            else
            {
                try
                {
                    var connectParameters = new ConnectParameters(false, true);
                    await _bluetoothAdapter.ConnectToDeviceAsync(selectedItem, connectParameters);          // if we are not connected, then try to connect to the BLE Device selected
                    await OnPrint(selectedItem);                               // Navigate to the Services Page to show the services of the selected BLE Device
                }
                catch
                {
                    await DisplayAlert("Error connecting", $"Error connecting to BLE device: {selectedItem.Name ?? "N/A"}", "Retry");       // give an error message if it is not possible to connect
                }
            }

            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);         // switch off the "Isbusy" indicator
            await _bluetoothAdapter.DisconnectDeviceAsync(selectedItem);

        }

        private async Task OnPrint(IDevice device)
        {
            try
            {
                var services = await device.GetServicesAsync();

                IService serviceSelect = null;
                ICharacteristic characteristicSelect = null;


                foreach (var service in services)
                {
                    var characteristics = await service.GetCharacteristicsAsync();

                    foreach (var characteristic in characteristics)
                    {
                        if (characteristic.CanWrite)
                        {
                            serviceSelect = service;
                            characteristicSelect = characteristic;
                            break;
                        }
                    }
                }


                if (serviceSelect != null && characteristicSelect != null)
                {


                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(GetEscPosData());
                    await characteristicSelect.WriteAsync(Encoding.UTF8.GetBytes("--------------------------------"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(GetEscPosDataRight(DateTime.Now.ToString("dd/MMM/yyyy HH:mm", new CultureInfo("es-ES"))));
                    await characteristicSelect.WriteAsync(Encoding.UTF8.GetBytes("--------------------------------"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(GetEscPosDataLeft("Cliente:     Jhosef Reyes"));
                    await characteristicSelect.WriteAsync(GetEscPosDataLeft("NIT:         107137100"));
                    await characteristicSelect.WriteAsync(Encoding.UTF8.GetBytes("--------------------------------"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(Encoding.UTF8.GetBytes("CANT DESCRIPCION PRECIO   TOTAL"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(Encoding.UTF8.GetBytes("0.73 REGGULAR AS 34.19    25.00"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });

                    await characteristicSelect.WriteAsync(Encoding.UTF8.GetBytes("--------------------------------"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(GetEscPosDataRight("TOTAL   25.00"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(GetEscPosDataCenter("IMPUESTO IDP 3.36"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(GetEscPosDataCenter("Numero de Autorizacion:"));
                    await characteristicSelect.WriteAsync(GetEscPosDataCenter(Guid.NewGuid().ToString()));
                    await characteristicSelect.WriteAsync(Encoding.UTF8.GetBytes("--------------------------------"));
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });
                    await characteristicSelect.WriteAsync(new byte[] { 0x0A });

                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error Imprimiendo", ex.Message, "Entendido");
            }
        }

        // Function to construct your ESC/POS byte array based on your desired formatting (replace with your logic)
        byte[] GetEscPosData()
        {

            byte[] escposData = new byte[]
            {
              0x1B, 0x61, 0x01, // Set center alignment
              0x1B, 0x21, 0x40, // Enable bold mode
            }
            .Concat(Encoding.UTF8.GetBytes("Kelpie Solutions"))
            .Concat(new byte[] { 0x0A })
            .Concat(new byte[] { 0x0A })
            .Concat(new byte[] { 0x0A })
            .Concat(new byte[] { 0x0A }).ToArray();

            return escposData;
        }

        byte[] GetEscPosDataCenter(string text)
        {

            byte[] escposData = new byte[]
            {
              0x1B, 0x61, 0x01,
            }
            .Concat(Encoding.UTF8.GetBytes(text))
            .Concat(new byte[] { 0x0A }).ToArray();

            return escposData;
        }

        byte[] GetEscPosDataLeft(string text)
        {

            byte[] escposData = new byte[]
            {
                0x1B, 0x61, 0x00,
            }
            .Concat(Encoding.UTF8.GetBytes(text))
            .Concat(new byte[] { 0x0A }).ToArray();

            return escposData;
        }

        byte[] GetEscPosDataRight(string text)
        {
            byte[] escposData = new byte[]
            {
            // Set right alignment
            0x1B, 0x61, 0x02
            }
            .Concat(Encoding.UTF8.GetBytes(text))
            .Concat(new byte[] { 0x0A }) // Line Feed character (optional)
            .ToArray();

            return escposData;
        }

    }

}
