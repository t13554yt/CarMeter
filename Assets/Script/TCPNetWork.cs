using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TCPNetwork : MonoBehaviour {
	private const int WaitTime = 5;
	
	private Socket _listener;
	private Socket _socket;
	
	public bool IsLoop { get; set; }
	private Thread _dispatchThread;
	
	private Action<NetEvent> _handler;
	
	/// <summary>
	/// サーバーとして開始
	/// </summary>
	/// <param name="port"></param>
	/// <param name="connectionNum"></param>
	/// <returns></returns>
	public bool StartServer(int port, int connectionNum) {
		try {
			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_listener.Bind(new IPEndPoint(IPAddress.Any, port));
			_listener.Listen(connectionNum);
		} catch {
			return false;
		}
		
		_handler(NetEvent.StartServer);
		
		return LaunchThread();
	}
	
	public bool Connect(IPAddress ipaddress, int port) {
		// 既にサーバとして起動
		if (_listener != null) {
			return false;
			Debug.Log("Listener false");
		}
		
		bool result = false;
		try {
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.NoDelay = true;
			_socket.Connect(ipaddress, port);
			result = LaunchThread();
		}catch(SocketException){
			_socket = null;
		}
		
		if (_handler != null) {
			_handler(result ? NetEvent.Connect : NetEvent.Error);
		}
		
		return result;
	}
	
	private bool LaunchThread() {
		try {
			IsLoop = true;
			_dispatchThread = new Thread(new ThreadStart(Dispatch));
			_dispatchThread.Start();
		} catch {
			return false;
		}
		
		
		return true;
	}
	
	private void Dispatch() {
		while (IsLoop) {
			Accept();
			
			Thread.Sleep(WaitTime);
		}
	}
	
	private void Accept() {
		if (_listener != null && _listener.Poll(0, SelectMode.SelectRead)) {
			_socket = _listener.Accept();
			// クライアント接続を通知
			_handler(NetEvent.Connect);
		}
	}
	
	public void RegHandler(Action<NetEvent> act) {
		_handler = act;
	}
	
	public void ClearHandler() {
		_handler = null;
	}
}