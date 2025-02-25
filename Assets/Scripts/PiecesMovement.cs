using UnityEngine;

public class PiecesMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidad de movimiento de la bola
    public float rotationAngle = 90f; // �ngulo de inclinaci�n de la bola  
    [SerializeField] private float lineLength = 0.5f;

    private LineRenderer[] lines; // Array para almacenar las 6 l�neas
    private GameObject[] lineColliders; // Array para almacenar los colliders de las l�neas
    private Vector3[] directions; // Direcciones de las l�neas
    private bool linesVisible = false; // Indica si las l�neas est�n visibles
    private int selectedDirection = -1; // Direcci�n seleccionada (-1 = ninguna)
    private bool isMoving = false; // Indica si la bola est� en movimiento
    private bool isRotating = false;
    private bool translationMode = true; // true = traslaci�n, false = rotaci�n

    private CameraOrbit cameraOrbit;

    void Start()
    {
        // Inicializar las direcciones de las l�neas
        directions = new Vector3[]
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        // Inicializar el array de l�neas y colliders
        lines = new LineRenderer[6];
        lineColliders = new GameObject[6];

        // Crear las l�neas y los colliders
        for (int i = 0; i < 6; i++)
        {
            GameObject lineObject = new GameObject("Line_" + i);
            lines[i] = lineObject.AddComponent<LineRenderer>();
            lines[i].positionCount = 5; // 5 puntos para la l�nea con punta de flecha
            lines[i].startWidth = 0.05f;
            lines[i].endWidth = 0.05f;
            lines[i].material = new Material(Shader.Find("Sprites/Default")); // Material b�sico
            lines[i].startColor = Color.green;
            lines[i].endColor = Color.green;
            lines[i].enabled = false; // Ocultar al inicio

            // Crear el collider para la l�nea
            lineColliders[i] = new GameObject("LineCollider_" + i);
            lineColliders[i].transform.SetParent(lineObject.transform);
            BoxCollider collider = lineColliders[i].AddComponent<BoxCollider>();
            collider.isTrigger = true; // No debe afectar la f�sica
            lineColliders[i].SetActive(false);
        }

        // Obtener la referencia al script CameraOrbit
        cameraOrbit = Camera.main.GetComponent<CameraOrbit>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            translationMode = true;
            Debug.Log("Modo: Traslaci�n");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            translationMode = false;
            Debug.Log("Modo: Rotaci�n");
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    ShowLines();
                    cameraOrbit.SetTarget(this.transform); // Cambiar el objetivo de la c�mara a esta pieza
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (hit.collider.gameObject == lineColliders[i])
                        {
                            selectedDirection = i;
                            if (translationMode)
                            {
                                isMoving = true;
                            }
                            else
                            {
                                isRotating = true;
                            }
                            linesVisible = false;
                            HideLines();
                            break;
                        }
                    }
                }
            }
        }

        if (isMoving)
        {
            MovePiece();
        }

        if (isRotating)
        {
            RotatePiece();
        }
    }

    void ShowLines()
    {
        HideLines();

        if (translationMode)
        {
            // Mostrar todas las flechas de traslaci�n (VERDES)
            for (int i = 0; i < 6; i++)
                DrawArrow(lines[i], directions[i], Color.green);
        }
        else
        {
            // Mostrar solo las flechas de rotaci�n (ROJAS)
            DrawArrow(lines[0], Vector3.up, Color.red); // Rotaci�n en Y (arriba/abajo)
            DrawArrow(lines[2], Vector3.left, Color.red); // Rotaci�n en X (izquierda/derecha)
            DrawArrow(lines[4], Vector3.forward, Color.red); // Rotaci�n en Z (adelante/atr�s)
        }

        linesVisible = true;
    }

    void DrawArrow(LineRenderer line, Vector3 direction, Color color)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + direction * lineLength;

        line.enabled = true;
        line.positionCount = 5;
        line.startColor = color;
        line.endColor = color;

        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);

        Vector3 arrowHeadBase = endPos - direction * (lineLength * 0.2f);
        Vector3 arrowLeft = arrowHeadBase + Quaternion.Euler(0, 45, 0) * (-direction * 0.1f);
        Vector3 arrowRight = arrowHeadBase + Quaternion.Euler(0, -45, 0) * (-direction * 0.1f);

        line.SetPosition(2, arrowLeft);
        line.SetPosition(3, endPos);
        line.SetPosition(4, arrowRight);

        GameObject lineCollider = lineColliders[System.Array.IndexOf(directions, direction)];
        lineCollider.SetActive(true);
        lineCollider.transform.position = transform.position + direction * (lineLength / 2);
        lineCollider.transform.rotation = Quaternion.LookRotation(direction);
        lineCollider.GetComponent<BoxCollider>().size = new Vector3(0.1f, 0.1f, lineLength);
    }

    void HideLines()
    {
        for (int i = 0; i < 6; i++)
        {
            lines[i].enabled = false;
            lineColliders[i].SetActive(false);
        }
    }

    void MovePiece()
    {
        if (selectedDirection != -1)
        {
            Vector3 movement = directions[selectedDirection] * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    void RotatePiece()
    {
        if (selectedDirection != -1)
        {
            Vector3 direction = directions[selectedDirection];

            // Inclinamos la bola hacia la direcci�n seleccionada
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, direction) * transform.rotation;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationAngle);

            isRotating = false; // Detenemos la rotaci�n despu�s de un solo paso
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isMoving = false;
        isRotating = false;
        selectedDirection = -1;
    }
}
