using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Misc;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.ChannelTargets
{
    public class ChannelWebhookTarget : ChannelTarget
    {
        public string url;
        public string body;
        public string jsonArg;
        public int timeout;
        public bool showPopup;
        public WebRequestController.RequestType requestType = WebRequestController.RequestType.GET;

        public ChannelWebhookTarget() : base(CHANNELTARGETS.WEBHOOK)
        {
        }
        
        public void SetUrl(string url)
        {
            this.url = url;
            linkedChannel?.Save();
        }

        public void SetBody(string body)
        {
            this.body = body;
            linkedChannel?.Save();
        }

        public void SetJsonArg(string jsonArg)
        {
            this.jsonArg = jsonArg;
            linkedChannel?.Save();
        }

        public void SetType(WebRequestController.RequestType requestType)
        {
            this.requestType = requestType;
            linkedChannel?.Save();
        }

        public void SetTimeout(int timeout)
        {
            this.timeout = timeout;
            linkedChannel?.Save();
        }

        public void SetPopup(bool popup)
        {
            this.showPopup = popup;
            linkedChannel?.Save();
        }
        
        public override async void Execute()
        {
            // we send the request
            UnityWebRequest request = await WebRequestController.SendJsonWebRequest(requestType, url, body, null, timeout, true);
            
            // and if we want the popup, we show it
            if (showPopup)
            {
                if (string.IsNullOrEmpty(jsonArg))
                {
                    PopupController.ShowPopup(request.downloadHandler.text);
                }
                else
                {
                    // we try to parse it and show it
                    try
                    {
                        string output = "";
                        
                        JToken obj = JObject.Parse(request.downloadHandler.text);
                        JToken current = obj;
                        
                        foreach (string arg in ParseArgs())
                        {
                            if (arg.StartsWith("\""))
                            {
                                // it's a string. we cut the object here and print whatever we want
                                if (current != obj)
                                {
                                    output += current.ToString();
                                    current = obj;
                                }
                                output += Regex.Unescape(arg.Substring(1, arg.Length - 2));
                            }
                            else if (arg.StartsWith("["))
                            {
                                // it's an array accessor. we extract the number
                                int number = int.Parse(arg.Substring(1, arg.Length - 2));
                                current = ((JArray)current)[number];
                            }
                            else
                            {
                                // it's a regular element accesor
                                current = current[arg];   
                            }
                        }

                        if (current != obj)
                        {
                            // we print it as well
                            output += current.ToString();
                        }
                        
                        PopupController.ShowPopup(output);
                    }
                    catch (Exception e)
                    {
                        PopupController.ShowPopup("Invalid JSON or reply argument!");
                    }
                }
            }
        }

        private string[] ParseArgs()
        {
            return jsonArg.Split(';');
        }

        public override string GetTag()
        {
            return url;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(url);
        }
    }
}