This tutorial will walk you through the creation 

![Azure architecture](./media/simulatorarch.svg){: width=50}

### Sign in to Azure

Sign in to the [Azure portal](https://portal.azure.com) with your Azure account.

### Create an IoT Central Application for your simulation
1. follow [these instructions to build a custom IoT Central Application](https://docs.microsoft.com/en-us/azure/iot-central/core/quick-deploy-iot-central).
### Add a device capability model.  
Our simulated device will send Location and Speed information.
1. select Device Template -> "+"
![](https://docs.microsoft.com/en-us/azure/iot-central/core/media/quick-create-simulated-device/device-definitions.png)
1. Choose "IoT Device" for template type.  Do not choose IoT Edge or the plug and play models.
1. Add a property called "Location".  This should be set as a GeoCoordinate type.
1. Add another property called "Speed".  This should be a double value.

### Create infrastructure : signalr service, single page website, and event hub

1. Create a resource group for our solution

    `$ az group create -n mysimulation -l westus2`
1. Create a storage account to act as a webserver.

    ```bash
    $ az storage account create \
            --name mysimulationweb \
            --resource-group mysimulation \
            --location westus2 \
            --sku Standard_LRS \
            --kind StorageV2  
    ```

    When this command completes review the `primaryEndpoints` web property.  You'll need this domain to set CORS policy in the SignalR service.  You can also reference the value in the portal later.  Here's an example of what you are looking for..

    `"web": "https://mysimulationweb.z5.web.core.windows.net/"`
  

1. enable SPA hosting.  The web property above will not return content until single page support is enabled and you've specified a default index and error page.

    ```bash
    $ az storage blob service-properties update  \
        --account-name  mysimulationweb \
        --static-website \
        --404-document error.html \
        --index-document index.html
    ```

1. copy files from solution into the $web container.  If you already have the Azure Storage Explorer installed it would be a good choice, otherwise you can accomplish this from the azure portal.  (note that we still need to update index.html with your signalr endpoint later)

1. Create a signalR service.  It's important to use service-mode "Serverless" for integration with our Azure Function later on.

    ```bash
    $ az signalr create \
      --name mysignalrservice \
      --resource-group mysimulation \
      --sku Standard_S1 \
      --unit-count 1 \
      --service-mode Serverless
    
    ```

1. Get your signalR connection string.  You'll find this in the portal as well.  From the shell...

    ```bash
    $ az signalr key list --name mysignalrservice \
      --resource-group mysimulation --query primaryConnectionString -o tsv
    
    ```
