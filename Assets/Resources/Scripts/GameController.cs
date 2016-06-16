using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
	public static _Instance {get; private set;}
	
	public void checkIfInstanceExists()
	{
		if(_Instance != this)
			Destroy(this.gameObject);
		else
			_Instance = this;
	}
	
	public void Awake()
	{
		checkIfInstanceExists();
	}
	
	public void Start()
	{
		checkIfInstanceExists();
	}
}