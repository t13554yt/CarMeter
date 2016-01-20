using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Net;

public class Chat : MonoBehaviour {
	// メッセージを管理するリスト
	private List<string> messages = new List<string>();
	// Chat用のテキスト
	private string currentMessage = string.Empty;
	// Server
	NetworkStream stream = null;
	bool isStopReading = false;
	byte[] readbuf;
	
	private IEnumerator Start(){
		Debug.Log("START START");
		readbuf = new byte[1024];
		
		while (true) {
			if(!isStopReading){StartCoroutine(ReadMessage ());}
			yield return null;
		}
	}
	
	private void OnGUI()
	{
		GUILayout.Space(10);
		GUILayout.BeginHorizontal(GUILayout.Width (250));
		
		// 入力情報取得
		currentMessage = GUILayout.TextField(currentMessage);
		
		// Sendボタン
		if ( GUILayout.Button("Send") )
		{
			// 入力が空ではない場合処理
			if ( !string.IsNullOrEmpty(currentMessage.Trim ()) && currentMessage != "")
			{
				Debug.Log(currentMessage);
				
				// Chatサーバに送信
				StartCoroutine(SendMessage (currentMessage));
				
				// 送信後は、入力値を空
				currentMessage = string.Empty;
			}
		}
		
		GUILayout.EndHorizontal();
		
		// Chat欄の生成
		createMessage (messages);
	}
	
	private void createMessage(List<string> messages){
		// 入力されたメッセージを逆順に100表示
		int count = 1;
		for ( int i=messages.Count-1; i>=0; i-- )
		{
			GUILayout.Label(messages[i]);
			count ++;
			if (count > 100)break;
		}
		
	}
	
	private IEnumerator SendMessage(string message){
		Debug.Log ("START SendMessage:" + message);
		
		if (stream == null) {
			stream = GetNetworkStream();
		}
		string playerName = "[A]: ";
		//サーバーにデータを送信する
		Encoding enc = Encoding.UTF8;
		byte[] sendBytes = enc.GetBytes(playerName + message + "\n");
		//データを送信する
		stream.Write(sendBytes, 0, sendBytes.Length);
		yield break;
	}
	
	private IEnumerator ReadMessage(){
		stream = GetNetworkStream ();
		// 非同期で待ち受けする
		stream.BeginRead (readbuf, 0, readbuf.Length, new AsyncCallback (ReadCallback), null);
		isStopReading = true;
		yield return null;
	}
	
	public void ReadCallback(IAsyncResult ar ){
		Encoding enc = Encoding.UTF8;
		stream = GetNetworkStream ();
		int bytes = stream.EndRead(ar);
		string message = enc.GetString (readbuf, 0, bytes);
		message = message.Replace("\r", "").Replace("\n", "");
		isStopReading = false;
		messages.Add(message);
	}   
	
	private NetworkStream GetNetworkStream(){
		if (stream != null && stream.CanRead) {
			return stream;
		}
		
		string ipOrHost = "127.0.0.1";
		int port = 10021;
		
		//TcpClientを作成し、サーバーと接続する
		TcpClient tcp = new TcpClient(ipOrHost, port);
		Debug.Log("success conn server");
		
		//NetworkStreamを取得する
		return tcp.GetStream();
	}
	
	private Socket GetSocket(){
		IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 10021);
		Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
		listener.Bind(localEndPoint);
		listener.Listen(10);
		return listener;
	}
}