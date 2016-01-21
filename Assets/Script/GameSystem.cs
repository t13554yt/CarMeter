using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System;

public class GameSystem : MonoBehaviour {
	private enum State {
		None,
		StartServer,
		WaitClient,
		ConnectToHost,
		Connecting,
	}

	public IdleChanger _idleChanger;

	private const int _PORT = 50000;
	private IPAddress _hostAddress;
	private TCPNetwork _tcpNetwork;
	
	// ステータスを表示するテキスト
	private Text _statusText;
	
	// 現在ステータス
	private State _state = State.None;
	
	// 通知されたイベント
	private NetEvent _netEvent = NetEvent.None;
	
	void Start() {
		_statusText = GameObject.Find("StatusText").GetComponent<Text>();
		
		IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
		_hostAddress = entry.AddressList[0];
		
		GameObject networkObj = new GameObject("Network");
		_tcpNetwork = networkObj.AddComponent<TCPNetwork>();
		_tcpNetwork.RegHandler(NetworkEventHandler);
	}
	
	void Update() {
		switch (_state) {
		case State.None: break;
		case State.StartServer: UpdateStartServer(); break;
		case State.WaitClient: UpdateWaitClient(); break;
		case State.ConnectToHost: UpdateConnectToHost(); break;
		case State.Connecting: UpdateConnecting(); break;
		default: throw new ArgumentOutOfRangeException(_state.ToString());
		}
	}
	
	// 初回の処理
	private void Step(State state) {
		_state = state;
		
		switch (_state) {
		case State.None: break;
		case State.StartServer: StepStartServer(); break;
		case State.WaitClient: StepWaitClient(); break;
		case State.ConnectToHost: StepConnectToHost(); break;
		case State.Connecting: StepConnecting(); break;
		default: throw new ArgumentOutOfRangeException(_state.ToString());
		}
	}
	
	private void StepStartServer() {
		_statusText.text = "運転中";
		
		_tcpNetwork.StartServer(_PORT, 1);
	}
	
	private void UpdateStartServer() {
		if (_tcpNetwork.IsLoop) {
			Step(State.WaitClient);
		}
	}
	
	private void StepWaitClient() {
	}
	
	private void UpdateWaitClient() {
		if (_netEvent == NetEvent.Connect) {
			Step(State.Connecting);
		}
	}
	
	private void StepConnectToHost() {
		if (!_tcpNetwork.IsLoop) {
			_tcpNetwork.Connect(_hostAddress, _PORT);
		} else {
			NetworkEventHandler(NetEvent.Error);
		}
	}
	
	private void UpdateConnectToHost() {
		if (_tcpNetwork.IsLoop) {
			Step(State.Connecting);
		}
		else{
			Debug.Log("isLoop flase");
		}
	}
	
	private void StepConnecting() {
		_statusText.text = "運転教えてくれてありがとう！！また教えてね！！";
		_idleChanger.Next();

	}
	
	private void UpdateConnecting() {
		
	}
	
	// StartServerボタン
	public void ClickStartServer() {
		Step(State.StartServer);
	}
	
	// ConnectToHostボタン
	public void ClickConnectToHost() {
		Step(State.ConnectToHost);
	}
	
	private void NetworkEventHandler(NetEvent netevent) {
		_netEvent = netevent;
	}
	
	
}