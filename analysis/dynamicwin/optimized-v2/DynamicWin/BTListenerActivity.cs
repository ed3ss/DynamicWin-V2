using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FenUISharp;
using FenUISharp.Logging;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace DynamicWin;

public class BTListenerActivity
{
	public static DeviceInformation? ConnectedDevice { get; private set; } = null;

	public static int ConnectedDeviceBattery { get; private set; } = -1;

	public static Action<DeviceInformation>? OnDeviceConnect { get; set; }

	public static Action? OnDeviceDisconnect { get; set; }

	public static Action<int>? BatteryInfoReceived { get; set; }

	private async Task GetBluetoothDeviceList()
	{
		DeviceInformationCollection deviceInformationCollection = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected));
		bool num = ConnectedDevice != null;
		if (deviceInformationCollection != null && deviceInformationCollection.Count > 0)
		{
			ConnectedDevice = deviceInformationCollection[0];
		}
		else
		{
			ConnectedDevice = null;
			ConnectedDeviceBattery = -1;
		}
		if (num && ConnectedDevice == null)
		{
			DynamicWin.MainWindow.LogicDispatcher.Invoke(delegate
			{
				OnDeviceDisconnect?.Invoke();
			});
		}
		if (!num && ConnectedDevice != null)
		{
			DynamicWin.MainWindow.LogicDispatcher.Invoke(delegate
			{
				OnDeviceConnect?.Invoke(ConnectedDevice);
			});
		}
		UpdateBattery();
	}

	private async void UpdateBattery()
	{
		if (!(ConnectedDevice == null))
		{
			int? battery = await GetBatteryLevelViaProcess(ConnectedDevice.Name);
			DynamicWin.MainWindow.LogicDispatcher.Invoke(delegate
			{
				BatteryInfoReceived?.Invoke(battery ?? (-1));
			});
			ConnectedDeviceBattery = battery ?? (-1);
		}
	}

	public async Task<int?> GetBatteryLevelViaProcess(string deviceName)
	{
		_ = 1;
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "powershell.exe",
				Arguments = "-Command \"\r\n                        $BTHDevices = Get-PnpDevice -FriendlyName '*" + deviceName + "*';\r\n                        if ($BTHDevices) {\r\n                            foreach ($Device in $BTHDevices) {\r\n                                $BatteryProperty = Get-PnpDeviceProperty -InstanceId $Device.InstanceId -KeyName '{104EA319-6EE2-4701-BD47-8DDBF425BBE5} 2' | Where-Object { $_.Type -ne 'Empty' } | Select-Object -ExpandProperty Data;\r\n                                if ($BatteryProperty) { Write-Output $BatteryProperty; break; }\r\n                            }\r\n                        }\r\n                    \"",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			using (Process process = Process.Start(startInfo))
			{
				if (process != null)
				{
					string output = await process.StandardOutput.ReadToEndAsync();
					await process.WaitForExitAsync();
					if (int.TryParse(output.Trim(), out var result))
					{
						return result;
					}
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			FLogger.Error("Process execution failed: " + ex.Message);
			return null;
		}
	}

	private void StartContinousBatteryUpdate()
	{
		DynamicWin.MainWindow.LogicDispatcher.InvokeLater(delegate
		{
			StartContinousBatteryUpdate();
		}, 60f);
		if (ConnectedDevice != null)
		{
			Task.Run(delegate
			{
				UpdateBattery();
			});
		}
	}

	public BTListenerActivity()
	{
		Task.Run(async delegate
		{
			try
			{
				await GetBluetoothDeviceList();
			}
			catch (Exception ex)
			{
				FLogger.Error("Error in GetBluetoothDeviceList: " + ex.Message);
			}
		});
		FWindowCallbacks callbacks = DynamicWin.MainWindow.Callbacks;
		callbacks.OnDevicesChanged = (Action)Delegate.Combine(callbacks.OnDevicesChanged, (Action)delegate
		{
			Task.Run(async delegate
			{
				try
				{
					await GetBluetoothDeviceList();
				}
				catch (Exception ex)
				{
					FLogger.Error("Error in OnDevicesChanged: " + ex.Message);
				}
			});
		});
		StartContinousBatteryUpdate();
		OnDeviceConnect = (Action<DeviceInformation>)Delegate.Combine(OnDeviceConnect, (Action<DeviceInformation>)delegate
		{
			ActivityManager.RegisterView<BTConnectedView>();
		});
		OnDeviceDisconnect = (Action)Delegate.Combine(OnDeviceDisconnect, (Action)delegate
		{
			ActivityManager.UnregisterView<BTConnectedView>();
		});
	}
}
