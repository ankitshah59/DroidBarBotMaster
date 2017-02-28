using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Java.IO;
using System.IO;
using Android.Media;
using Java.Util;
using System.Threading;

namespace DroidBarBotMaster.Droid.Class.Service
{
    public class MyBluetoothService
    {
        private BluetoothAdapter mBluetoothAdapter;
        private BluetoothDevice mbarBotDevice;
        private BluetoothSocket mSocket;
        private System.IO.Stream mInStream;
        private System.IO.Stream mOutStream;
        private Toast toastMessenger;
        private Context context;



        public MyBluetoothService(Context context)
        {
            this.context = context;
            toastMessenger = new Toast(this.context);

            DeviceHasBT();
            EnableBTdevice();
            GetBondedDevices();
            SocketConnect();
            InOutSocketInit();
            ShowToastMessage("CONNECTED TO BARBOT");
        }


        #region Write, Read
        public async void Read()
        {
            byte[] mmBuffer = new byte[1024];
            int numBytes; // bytes returned from read()

            // Keep listening to the InputStream until an exception occurs.
            while (true)
            {
                try
                {

                    // Read from the InputStream.
                    numBytes = await mInStream.ReadAsync(mmBuffer, 0, mmBuffer.Length);
                    // Send the obtained bytes to the UI activity.
                    if (numBytes > 5)
                    {
                        System.Console.WriteLine(ASCIIEncoding.ASCII.GetString(mmBuffer));
                    }

                }
                catch (System.IO.IOException e)
                {
                    System.Console.WriteLine("InputStream failure ERROR:4084");
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        //Call this from the main activity to send data to the remote device.
        public async void Write(byte[] bytes)
        {
            try
            {
                await mOutStream.WriteAsync(bytes, 0, bytes.Length);
                System.Console.WriteLine("--SEND--");
                System.Console.WriteLine(ASCIIEncoding.ASCII.GetString(bytes));

            }
            catch (System.IO.IOException e)
            {
                System.Console.WriteLine("Could not SEND Error:6548");
                System.Console.WriteLine(e.Message);

                throw;
            }
        }




        #endregion

        #region Initialize Bluetooth sequence
        public bool DeviceHasBT()
        {
            mBluetoothAdapter = mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (mBluetoothAdapter == null)
            {
                toastMessenger = Toast.MakeText(context, "Error no BT found on device", ToastLength.Long);
                toastMessenger.Show();
                return false;
            }
            else
            {
                toastMessenger = Toast.MakeText(context, "Got adapter", ToastLength.Long);
                toastMessenger.Show();
            }
            return true;

        }

        public bool EnableBTdevice()
        {
            int REQUEST_ENABLE_BT = 0;


            // Enable BT
            if (!mBluetoothAdapter.IsEnabled)
            {
                Intent enableBT = new Intent(BluetoothAdapter.ActionRequestEnable);
                Activity s = new Activity();
                s.StartActivityForResult(enableBT, REQUEST_ENABLE_BT);

                // TODO: Check for cancelaion

                //OnActivityResult(REQUEST_ENABLE_BT, Result.Canceled, enableBT);
                //if (REQUEST_ENABLE_BT == 0)
                //{
                //    toastMessenger = Toast.MakeText(this, "BT not enabled", ToastLength.Long);
                //    toastMessenger.Show();
                //    return;
                //}
            }
            toastMessenger = Toast.MakeText(context, "Enabled BT", ToastLength.Long);
            toastMessenger.Show();
            return true;
        }

        public bool GetBondedDevices()
        {
            // Get Bonded Devices
            mbarBotDevice = (from x in mBluetoothAdapter.BondedDevices
                                where x.Name.ToLower() == ("BarBot").ToLower()
                                select x).FirstOrDefault();

            if (mbarBotDevice == null)
            {
                toastMessenger = Toast.MakeText(context, "Manual pair connect to BarBot please.", ToastLength.Long);
                toastMessenger.Show();
                return false;
            }
            else
            {
                toastMessenger = Toast.MakeText(context, "Found BarBot bond", ToastLength.Long);
                toastMessenger.Show();
                return true;
            }
        }

        public bool SocketConnect()
        {
            BluetoothSocket tmpSocket;
            // Set up socket
            try
            {
                UUID id = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                tmpSocket = mbarBotDevice.CreateInsecureRfcommSocketToServiceRecord(id);
            }
            catch (System.IO.IOException e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine("Socket's listen() method failed");
                toastMessenger = Toast.MakeText(context, "Found BarBot bond", ToastLength.Long);
                toastMessenger.Show();
                return false;
            }

            mSocket = tmpSocket;


            // Drive on a diffrent Socket
            mBluetoothAdapter.CancelDiscovery();
            try
            {
                mSocket.Connect();
            }
            catch (Exception e)
            {
                try
                {
                    mSocket.Close();
                    System.Console.WriteLine("Closing connection");

                }
                catch (Exception r)
                {
                    System.Console.WriteLine("Could not close the client socket");
                    return false;
                }
            }
            Thread.Sleep(1500);
            return true;
        }

        private void InOutSocketInit()
        {

            // Get the input and output streams; using temp objects because
            // member streams are .
            try
            {
                mInStream = mSocket.InputStream;
            }
            catch (System.IO.IOException e)
            {
                System.Console.WriteLine("InputStream Socket fail to establish ERROR:4856");
                System.Console.WriteLine(e.Message);
                return;
            }


            try
            {
                mOutStream = mSocket.OutputStream;
            }
            catch (System.IO.IOException e)
            {
                System.Console.WriteLine("OutputStream Socket fail to establish ERROR:4857");
                System.Console.WriteLine(e.Message);
                return;
            }

        }

        // Call this method from the main activity to shut down the connection.
        public void cancelSocketServ()
        {
            try
            {
                mSocket.Close();
            }
            catch (System.IO.IOException e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine("Could not cancel socket");
                throw;
            }
        }

        #endregion

        private void ShowToastMessage(String message)
        {
            toastMessenger = Toast.MakeText(context, message, ToastLength.Long);
            toastMessenger.Show();
        }

    }
}