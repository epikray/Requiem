using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DetectionField : MonoBehaviour
{
    // Start is called before the first frame update

    private Collider m_Collider;
    private bool _playerInside = false;
    private bool _charsInFieldIsDirty = true;
    private bool _playerRefIsDirty = true;
    
    [SerializeField]
    private Vector3 _playerLastLocation;
    private GameObject _playerRef;
    [SerializeField]
    private List<GameObject> _charsInField;
    public List<GameObject> charsInField {
        get {
            return _charsInField;
        }
    }
    public bool playerInside {
        get { 
            if(_charsInFieldIsDirty) _playerInside = HasPlayerInside();

            return _playerInside; 
        }
    }

    public Vector3 playerLastLocation {
        get {
            return _playerLastLocation;
        }
    }

    public GameObject playerRef {
        get {
            if(_playerRefIsDirty) _playerRef = FindPlayerObjectRef();
            return _playerRef;
        }
    }

    protected void Awake() {
        _charsInField = new List<GameObject>();
        _playerRef = null;
        _playerLastLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    protected void Start()
    {  
        
        m_Collider = GetComponent<Collider>();
        
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    bool HasPlayerInside() {
        _charsInFieldIsDirty = false;
        return _charsInField.Exists(
            (GameObject G) => {
                if(G.tag == "Player") return true; 
                return false;
            });
    }

    GameObject FindPlayerObjectRef() {
        _playerRefIsDirty = false;
        return _charsInField.Find(
            (GameObject G) => {
                if(G.tag == "Player") return true; 
                return false;
            });
    }

    void OnTriggerEnter(Collider other) {
        
        Behaviour otherBehaviour = null;
        if(other.gameObject.TryGetComponent<Behaviour>(out otherBehaviour)) {
            _charsInFieldIsDirty = true;
            _playerRefIsDirty = true;
            _charsInField.Add(other.gameObject);
        }
        
        if(other.gameObject.tag == "Player") {
            _playerInside = true;
            _playerRef = other.gameObject;
            _playerLastLocation = _playerRef.transform.position;
        }
    }

    void OnTriggerStay(Collider other) {
        if(other.gameObject.tag == "Player") {
            //_playerInside = true;
            _playerLastLocation = _playerRef.transform.position;
        }
    }

    void OnTriggerExit(Collider other) {
        Behaviour otherBehaviour = null;
        if(other.gameObject.TryGetComponent<Behaviour>(out otherBehaviour)) {
            _charsInFieldIsDirty = true;
            _playerRefIsDirty = true;
            _charsInField.Remove(other.gameObject);
        }

        if(other.gameObject.tag == "Player") {
            _playerInside = false;
            _playerRef = null;
            _playerLastLocation = other.gameObject.transform.position;
        }
    }
}
