using System;
using System.IO.Ports;
using System.Threading;

public class Program {
    public static void Main(string[] args) {
        BluetoothManager btManager = new BluetoothManager();
        Console.ReadLine();
        btManager.Stop();
    }
}