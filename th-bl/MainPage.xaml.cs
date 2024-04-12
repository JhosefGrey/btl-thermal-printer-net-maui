using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.iOS;

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
        }

        private async Task OnPrint(IDevice device)
        {
            try
            {

                var services = await device.GetServicesAsync();

                var characteristics = await services.FirstOrDefault().GetCharacteristicsAsync();

                //await _bluetoothAdapter.DisconnectDeviceAsync(device);
                //var printer = new SerialPrinter(portName: device.Name, baudRate: 115200);

                //var e = new EPSON();
                //printer.Write( // or, if using and immediate printer, use await printer.WriteAsync
                //  ByteSplicer.Combine(
                //    e.CenterAlign(),
                //    //e.PrintImage(File.ReadAllBytes("images/pd-logo-300.png"), true),
                //    e.PrintLine(""),
                //    e.SetBarcodeHeightInDots(360),
                //    e.SetBarWidth(BarWidth.Default),
                //    e.SetBarLabelPosition(BarLabelPrintPosition.None),
                //    e.PrintBarcode(BarcodeType.ITF, "0123456789"),
                //    e.PrintLine(""),
                //    e.PrintLine("B&H PHOTO & VIDEO"),
                //    e.PrintLine("420 NINTH AVE."),
                //    e.PrintLine("NEW YORK, NY 10001"),
                //    e.PrintLine("(212) 502-6380 - (800)947-9975"),
                //    e.SetStyles(PrintStyle.Underline),
                //    e.PrintLine("www.bhphotovideo.com"),
                //    e.SetStyles(PrintStyle.None),
                //    e.PrintLine(""),
                //    e.LeftAlign(),
                //    e.PrintLine("Order: 123456789        Date: 02/01/19"),
                //    e.PrintLine(""),
                //    e.PrintLine(""),
                //    e.SetStyles(PrintStyle.FontB),
                //    e.PrintLine("1   TRITON LOW-NOISE IN-LINE MICROPHONE PREAMP"),
                //    e.PrintLine("    TRFETHEAD/FETHEAD                        89.95         89.95"),
                //    e.PrintLine("----------------------------------------------------------------"),
                //    e.RightAlign(),
                //    e.PrintLine("SUBTOTAL         89.95"),
                //    e.PrintLine("Total Order:         89.95"),
                //    e.PrintLine("Total Payment:         89.95"),
                //    e.PrintLine(""),
                //    e.LeftAlign(),
                //    e.SetStyles(PrintStyle.Bold | PrintStyle.FontB),
                //    e.PrintLine("SOLD TO:                        SHIP TO:"),
                //    e.SetStyles(PrintStyle.FontB),
                //    e.PrintLine("  FIRSTN LASTNAME                 FIRSTN LASTNAME"),
                //    e.PrintLine("  123 FAKE ST.                    123 FAKE ST."),
                //    e.PrintLine("  DECATUR, IL 12345               DECATUR, IL 12345"),
                //    e.PrintLine("  (123)456-7890                   (123)456-7890"),
                //    e.PrintLine("  CUST: 87654321"),
                //    e.PrintLine(""),
                //    e.PrintLine("")
                //  )
                //);

            }
            catch (Exception ex)
            {

                await DisplayAlert("Error Imprimiendo", ex.Message, "Entendido");
            }
        }
    }

}
