using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MasterObjectCache : MonoBehaviour
{
	public static _Instance {get; private set;}
	
	public Aimer gameAimer {get; private set;}
	private List<Player> _Players;
	public ReadOnlyCollection<Player> players {get { return _Players.AsReadOnly(); } private set;}
	
	public void checkIfInstanceExists()
	{
		if(_Instance != this)
			Destroy(this.gameObject);
		else
			_Instance = this;
	}
	
	public void findObjects()
	{
		_Players = new List<Player>(FindObjectsOfType(typeof(Player)) as Player[]);
		gameAimer = FindObjectOfType(typeof(Aimer)) as Aimer;
	}
	
	public bool verifyObjects()
	{
		if(_Players == null)
		{
			Console.LogError("MasterObjectCache: Players is null");
			Debug.Break();
		}
			
		if(_Players.count == 0)
		{
			Console.LogError("MasterObjectCache: Players is empty");
			Debug.Break();
		}
		
		if(gameAimer == null)
		{
			Console.LogError("MasterObjectCache: Game Aimer is null");
			Debug.Break();
		}
	}
	
	public void Awake()
	{
		checkIfInstanceExists();
	}
	
	public void Start()
	{
		checkIfInstanceExists();
		_Players = new List<Player>();
	}
}

