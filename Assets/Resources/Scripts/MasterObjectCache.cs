using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MasterObjectCache : MonoBehaviour
{
	public static MasterObjectCache _Instance {get; private set;}
	
	public Aimer gameAimer {get; private set;}
	private List<Player> _Players;
	public ReadOnlyCollection<Player> players {get { return _Players.AsReadOnly(); }}
	
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
			Debug.LogError("MasterObjectCache: Players is null");
			Debug.Break();
			return false;
		}
			
		if(_Players.Count == 0)
		{
			Debug.LogError("MasterObjectCache: Players is empty");
			Debug.Break();
			return false;
		}
		
		if(gameAimer == null)
		{
			Debug.LogError("MasterObjectCache: Game Aimer is null");
			Debug.Break();
			return false;
		}

		return true;
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

