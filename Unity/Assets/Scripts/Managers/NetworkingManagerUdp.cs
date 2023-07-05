using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
public class NetworkingManagerUdp : MonoBehaviour
{
    UdpClient udpClient;
    IPEndPoint remoteEndPoint;

    void Start()
    {
        udpClient = new UdpClient(11000);
        remoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.10"), 11000);

        StartCoroutine(SendDataCoroutine());

        StartCoroutine(ReceiveDataCoroutine());
    }

    IEnumerator SendDataCoroutine()
    {
        string text = "Hello";
        byte[] send_buffer = Encoding.ASCII.GetBytes(text);
        udpClient.Send(send_buffer, send_buffer.Length, remoteEndPoint);

        yield return new WaitForSeconds(1f);

        StartCoroutine(SendDataCoroutine());
    }

    IEnumerator ReceiveDataCoroutine()
    {
        while (true)
        {
            Task<UdpReceiveResult> receiveTask = Task.Run(async () =>
            {
                return await udpClient.ReceiveAsync();
            });

            while (!receiveTask.IsCompleted)
            {
                yield return null; // wait until next frame
            }

            UdpReceiveResult result = receiveTask.Result;
            string receivedData = Encoding.ASCII.GetString(result.Buffer);
            Debug.Log("Received: " + receivedData);
        }
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
