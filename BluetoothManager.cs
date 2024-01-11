using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class BluetoothManager {

    private SerialPort serialPort;
    private SerialPort outputPort;
    private byte lastByte;
    private bool freshPacket = false;
    private bool inPacket = false;
    private int packetIndex = 0;
    private int packetLength = 0;
    private int checksum = 0;
    private int checksumAccumulator = 0;
    private int eegPowerLength = 0;
    private bool hasPower = false;
    private const int MAX_PACKET_LENGTH = 32;
    private const int EEG_POWER_BANDS = 8;
    private int signalQuality = 200;
    private int attention = 0;
    private int meditation = 0;

    private uint[] eegPower = new uint[EEG_POWER_BANDS];
    private byte[] packetData = new byte[MAX_PACKET_LENGTH];
    public BluetoothManager() {
        serialPort = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        outputPort = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);

        if (!serialPort.IsOpen) {
            serialPort.Open();
        }
        
        if (!outputPort.IsOpen) {
            Console.WriteLine(outputPort.IsOpen);
            outputPort.Open();
            Console.WriteLine(outputPort.IsOpen);
        }
    }

    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {

        SerialPort sp = (SerialPort) sender;
        bool parseSuccess;
        if (sp.IsOpen) {
            int bufferLen = sp.BytesToRead;
            byte[] buffer = new byte[bufferLen];
            sp.Read(buffer, 0, bufferLen);

            foreach (var currByte in buffer) {
                if (inPacket) {
                    if (packetIndex == 0) {
                        packetLength = currByte;

                        if (packetLength > MAX_PACKET_LENGTH) {
                            inPacket = false;
                        }
                    }
                    else if (packetIndex <= packetLength) {
                        //packet index - 1 because first byte is skipped
                        packetData[packetIndex - 1] = currByte;
                        checksumAccumulator += (currByte & 0xFF);
                    }
                    else if (packetIndex > packetLength) {
                        checksum = currByte;
                        checksumAccumulator &= 0xFF;
                        checksumAccumulator = (~checksumAccumulator & 0xFF);

                        if (checksum == checksumAccumulator) {
                            parseSuccess = parsePacket();
                            
                            if (parseSuccess) {
                                sendSignal();
                                freshPacket = true;
                            }
                            else {
                                //else parsing failed :(
                            }
                        }
                        else {
                            //else checksum mismatch :(
                        }
                        inPacket = false;
                    }
                    packetIndex++;
                }

                //look for start of packet
                if ((currByte == 0xAA) && (lastByte == 0xAA) && !inPacket) {
                    inPacket = true;
                    packetIndex = 0;
                    checksumAccumulator = 0;
                    clearEegPower();
                }
                lastByte = currByte;
            }
        }
        
        if (freshPacket) {
            freshPacket = false;
            return;
        }
    }

    private void clearEegPower() {
        for (byte i = 0; i < EEG_POWER_BANDS; i++) {
            eegPower[i] = 0;
        }
    }

    private bool parsePacket() {
        hasPower = false;
        clearEegPower();
        bool parseSuccess = true;
        int rawValue = 0;

        for (int i = 0; i < packetLength; i++) {

            switch (packetData[i]) {
                case (0x2):
                    //++i - pre-increment - a.k.a. also updates the value of i
                    //i++ - post-increment - updates value of i but returns original value
                    signalQuality = packetData[++i];
                    Console.WriteLine($"Signal Quality: {signalQuality}");
                    break;
                case (0x4):
                    attention = packetData[++i];
                    Console.WriteLine($"Focus: {attention}");
                    break;
                case (0x5):
                    meditation = packetData[++i];
                    Console.WriteLine($"Meditation: {meditation}");
                    break;
                case (0x83):
                    i++;
                    for (int j = 0; j < EEG_POWER_BANDS; j++) {
                        eegPower[j] = ((uint)packetData[++i] << 16) | ((uint)packetData[++i] << 8 | (uint)packetData[++i]);
                    }
                    hasPower = true;
                    break;
                case (0x80):
                    i++;
                    rawValue = ((int)packetData[++i] << 8 | packetData[++i]);
                    break;
                default:
                    parseSuccess = false;
                    break;
            }
        }
        return parseSuccess;
    }

    private void sendSignal() {
        if (attention > 55) {
            outputPort.Write("t");
        }
        else {
            outputPort.Write("f");
        }
    }

    public void Start() {
        serialPort.Open();
    }

    public void Stop() {
        serialPort.Close();
    }
    public void printValues() {
        Console.WriteLine($"Focus: {attention}");
        Console.WriteLine($"Meditation: {meditation}");
        Console.WriteLine($"Signal Quality: {signalQuality}");
    }
}