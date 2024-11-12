﻿using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler(IRandomisedValueProvider randomisedValueProvider, Socket clientSocket): IServerRequestHandler
{   
    private Socket ClientSocket = clientSocket;
    private string? Mpan;
    private readonly IRandomisedValueProvider _randomisedValueProvider = randomisedValueProvider;
    private EndPoint? _endPoint;
    public void CreateRequest(decimal initialReading, RequestType requestType, TariffType tariffType, int billingPeriod, decimal standingCharge)
    {
        try
        {
            var currentUsageRequest = new CurrentUsageRequest
            {
                TariffType = tariffType,
                CurrentReading = initialReading,
                BillingPeriod = billingPeriod,
                StandingCharge = standingCharge
            };

            initialReading += _randomisedValueProvider.GenerateRandomReading();
            SendRequest(requestType, currentUsageRequest);
        }
        catch
        {

        }
    }
    public void SendRequest<T>(RequestType requestType, T request) where T : IUsageRequest
    {
        try
        {
            // If the socket is null and the socket is not connected, throw an invalid operation exception (method cannot be performed).
            // Use this exception as you wish, this is a basic implementation.
            /*if (ClientSocket?.Connected != true)
            {
                ClientSocket.Connect(_endPoint);
            }*/

            if (Mpan is null || string.IsNullOrEmpty(Mpan))
            {
               Mpan=_randomisedValueProvider.GenerateMpanForClient();
            }

            // Create a new request.
            var serverRequest = new ServerRequest
            {
                Mpan = Mpan,
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(serverRequest));
            ClientSocket.SendAsync(data);

        }
        catch (Exception ex)
        {

        }
    }
    //Call this function right after the request is sent.
    public void GetResponse()
    {
        //check the requestype
        // get the response data, convert to string
        // populate the MeterReading Model class
        //display on Screen
    }
}
