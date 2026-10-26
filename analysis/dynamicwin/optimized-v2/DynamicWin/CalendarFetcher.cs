using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FenUISharp;
using FenUISharp.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace DynamicWin;

internal class CalendarFetcher
{
	private static string credPath = "gcalendar";

	public static List<CalendarEventStruct> Events { get; private set; } = new List<CalendarEventStruct>();

	public static Action? OnEventsUpdated { get; set; }

	public static void FetchCalendarEvents()
	{
		Events = new List<CalendarEventStruct>();
		Task.Run(() => FetchGoogleCalendar());
	}

	public static Stream GenerateStreamFromString(string s)
	{
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(s);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		return memoryStream;
	}

	public static void LogoutUserGoogle()
	{
		Task.Run(async delegate
		{
			await new FileDataStore(credPath, fullPath: true).ClearAsync();
		});
	}

	public static bool IsUserLoggedInGoogle()
	{
		FileDataStore fileDataStore = new FileDataStore(credPath, fullPath: true);
		if (!Directory.Exists(fileDataStore.FolderPath) || Directory.GetFiles(fileDataStore.FolderPath).Count() == 0)
		{
			return false;
		}
		return true;
	}

	public static async Task TryLoginGoogle(Action? onLogin = null)
	{
        string? credentialsFile = Environment.GetEnvironmentVariable("DYNAMICWIN_GOOGLE_CREDENTIALS");
        if (string.IsNullOrWhiteSpace(credentialsFile) || !File.Exists(credentialsFile))
        {
            FLogger.Error("Set DYNAMICWIN_GOOGLE_CREDENTIALS to your own OAuth client JSON file to use Google Calendar.");
            return;
        }
        string text = File.ReadAllText(credentialsFile);
		string[] scopes = new string[1] { CalendarService.Scope.CalendarReadonly };
		await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromStream(GenerateStreamFromString(text)).Secrets, scopes, "user", CancellationToken.None, new FileDataStore(credPath, fullPath: true));
		onLogin?.Invoke();
		FetchCalendarEvents();
	}

	private static void CreateDummyData()
	{
		Events = new List<CalendarEventStruct>
		{
			new CalendarEventStruct
			{
				name = "Test 1",
				at = DateTime.Today,
				end = DateTime.Today
			},
			new CalendarEventStruct
			{
				name = "Test 12d12 2",
				at = DateTime.Now.AddMinutes(10.0),
				end = DateTime.Now.AddMinutes(100.0)
			},
			new CalendarEventStruct
			{
				name = "Test awdawd 2",
				at = DateTime.Now.AddMinutes(25.0),
				end = DateTime.Now.AddMinutes(455.0)
			},
			new CalendarEventStruct
			{
				name = "Test awpodkapowdWD 3",
				at = DateTime.Today.AddDays(1.0),
				end = DateTime.Today.AddDays(1.0).AddMinutes(100.0)
			}
		};
	}

	public static async Task FetchGoogleCalendar()
	{
        string? credentialsFile = Environment.GetEnvironmentVariable("DYNAMICWIN_GOOGLE_CREDENTIALS");
        if (string.IsNullOrWhiteSpace(credentialsFile) || !File.Exists(credentialsFile))
        {
            FLogger.Error("Set DYNAMICWIN_GOOGLE_CREDENTIALS to your own OAuth client JSON file to use Google Calendar.");
            return;
        }
        string text = File.ReadAllText(credentialsFile);
		string[] scopes = new string[1] { CalendarService.Scope.CalendarReadonly };
		string appName = "DynamicWinV2";
		UserCredential httpClientInitializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromStream(GenerateStreamFromString(text)).Secrets, scopes, "user", CancellationToken.None, new FileDataStore(credPath, fullPath: true));
		EventsResource.ListRequest listRequest = new CalendarService(new BaseClientService.Initializer
		{
			HttpClientInitializer = httpClientInitializer,
			ApplicationName = appName
		}).Events.List("primary");
		listRequest.TimeMin = DateTime.UtcNow;
		listRequest.ShowDeleted = false;
		listRequest.SingleEvents = true;
		listRequest.MaxResults = 10;
		listRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
		try
		{
			Events events = await listRequest.ExecuteAsync();
			if (events.Items == null || events.Items.Count <= 0)
			{
				return;
			}
			foreach (Event item in events.Items)
			{
				DateTime dateTime = item.Start.DateTime ?? DateTime.Parse(item.Start.Date);
				DateTime dateTime2 = item.End.DateTime ?? DateTime.Parse(item.End.Date);
				DateTime now = DateTime.Now;
				if (dateTime2 > now && dateTime <= now.AddDays(30.0))
				{
					Events.Add(new CalendarEventStruct
					{
						name = item.Summary,
						at = dateTime,
						end = dateTime2
					});
				}
			}
		}
		catch (Exception ex)
		{
			FLogger.Error("Error fetching gcalendar. " + ex.Message);
		}
		finally
		{
			OnEventsUpdated?.Invoke();
		}
	}

	internal static void StartFetching(Dispatcher dispatcher)
	{
		dispatcher.InvokeLater(delegate
		{
			StartFetching(dispatcher);
		}, 1800f);
		if (IsUserLoggedInGoogle())
		{
			FetchCalendarEvents();
		}
	}
}

