using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour {


    public GameObject m_player;
    public float m_smoothSpeed = 0.125f;

    private Vector3 m_offset;

	
	void Start ()
    {
        m_offset = transform.position - m_player.transform.position;
	}

	void LateUpdate ()
    {
        Vector3 m_desiredPosition = m_player.transform.position + m_offset;
        Vector3 m_smoothedPosition = Vector3.Lerp(transform.position, m_desiredPosition, m_smoothSpeed);

        transform.position = m_smoothedPosition;
	}
}
