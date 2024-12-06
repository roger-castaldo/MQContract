using Messages;
using MQContract.AzureServiceBus;

var serviceConnection = new Connection(new("Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"));

await SampleExecution.ExecuteSample(serviceConnection, "AzureServiceBus");