using UnityEngine;
using System.Collections;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class PathFollower : MonoBehaviour
{

    private Vector3[] realVectors = new Vector3[66];
    private Vector3[] idealVectors = new Vector3[66];
    private Vector3 _direction;
    private Quaternion _lookRotation;
    public float speed = 25.0f;
    public float reachDist = 1.0f;
    public float rotationSpeed = 1.0f;
    public float deviationRange = 5.0f;
    private float dist;
    public int currentPoint = 0;
    public GameObject dangerIndicator;
    public GameObject clearIndicator;
    private static double Q = 0.000001;
    private static double R = 0.000004;
    private static double P = 1, X = 0, K;

    void Start()
    {
        //Failo nuskaitymas
        StreamReader inp_stm = new StreamReader("C:\\new 20.txt");

        int i = 0;
        //Ciklas vykdomas iki nuskaityto failo pabaigos
        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            String[] vectorArray = inp_ln.Split(' ');
            //Kiekvienai koordinatei sukuriamas Vector3 objektas ir kiekviena koordinatė yra išplėčiama atitinkamai koordinačių kiekiui (-i*10)
            Vector3 vector = new Vector3(float.Parse(vectorArray[0]) - i * 10, (float)Math.Round(updateKalman(float.Parse(vectorArray[2]))), float.Parse(vectorArray[1]) - i * 10);
            //Ši koordinatė patalpinama į sąrašą
            realVectors[i] = vector;
            i++;
        }

        //Lėktuvo priekis nukreipiamas pirmosios koordinatės link
        transform.position = realVectors[0];
        _direction = (realVectors[currentPoint] - transform.position).normalized;
        //Uždaromas nuskaitytas failas
        inp_stm.Close();

        //Failo nuskaitymas
        inp_stm = new StreamReader("C:\\koordinatės.txt");

        i = 0;
        //Ciklas vykdomas iki nuskaityto failo pabaigos
        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            String[] vectorArray = inp_ln.Split(' ');
            //Kiekvienai koordinatei sukuriamas Vector3 objektas ir kiekviena koordinatė yra išplėčiama atitinkamai koordinačių kiekiui (-i*10)
            Vector3 vector = new Vector3(float.Parse(vectorArray[0]) - i * 10, (float)Math.Round(updateKalman(float.Parse(vectorArray[2]))), float.Parse(vectorArray[1]) - i * 10);
            //Ši koordinatė patalpinama į sąrašą
            idealVectors[i] = vector;
            i++;
        }

        //Uždaromas nuskaitytas failas
        inp_stm.Close();
    }

    void Update()
    {
        if ((currentPoint + 1) < realVectors.Length)
        {
            //Nustatomas atstumas nuo esamos iki sekančios koordinatės
            dist = Vector3.Distance(realVectors[currentPoint], transform.position);
            //Objektas juda sekančio taško link
            transform.position = Vector3.MoveTowards(transform.position, realVectors[currentPoint], Time.deltaTime * speed);

            //Surandama kryptis nuo esamos koordinatės iki sekančios
            _direction = (realVectors[currentPoint] - transform.position).normalized;
            //Atliekamas objekto pasukimas tolimesnės koordinatės link
            if (_direction != Vector3.zero)
            {
                _lookRotation = Quaternion.LookRotation(_direction);
                //Objektas pasukamas atsižvelgiant į judesio greitį ir laiką
                transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * rotationSpeed);
            }

            if (dist <= reachDist)
            {
                currentPoint++;
                speed = speed - 0.1f;
            }

            checkVectorDeviation();
        }
        //Skaičiuojamas dabartinis taškas
        if (currentPoint >= realVectors.Length)
        {
            currentPoint = realVectors.Length - 1;
        }
    }

    void OnDrawGizmos()
    {
        int skipCount = 0;

        if (realVectors.Length > 0)
        {
            //Nuskaitytos trajektorijos ciklas
            for (int i = 0; i < realVectors.Length - 1; i++)
            {
                if (realVectors[i] != null)
                {
                    //Skrydžio trajektorijos linija
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(realVectors[i], realVectors[i + 1]);
                }
            }
        }

        skipCount = 0;

        if (idealVectors.Length > 0)
        {
            //Nuskaitytos trajektorijos ciklas
            for (int i = 0; i < idealVectors.Length - 1; i++)
            {
                if (idealVectors[i] != null)
                {
                    //Skrydžio trajektorijos apskritimai
#if UNITY_EDITOR
                    Handles.color = Color.green;

                    Handles.DrawWireDisc(idealVectors[i], new Vector3(1f, 1f, 4f), deviationRange*2);
                    /*Apskritimų piešimo dažnio sumažinimas
                    if (skipCount == 3) Handles.DrawWireDisc(idealVectors[i], new Vector3(1f, 1f, 4f), deviationRange);
                    skipCount++;*/
#endif
                   /* if (skipCount > 3) skipCount = 0;
                    */
                }
            }
        }
    }

    //Koordinatės skaičiavimas naudojant Kalmano filtra 
    private void measurementUpdate()
    {
        K = (P + Q) / (P + Q + R);
        P = R * (P + Q) / (R + P + Q);
    }

    public double updateKalman(double measurement)
    {
        measurementUpdate();
        double result = X + (measurement - X) * K;
        X = result;
        return result;
    }

    private void changeIndicator(bool dangerActive)
    {
        dangerIndicator.SetActive(dangerActive);
        clearIndicator.SetActive(!dangerActive);
    }

    private void checkVectorDeviation()
    {
        Vector3 currentVector = realVectors[currentPoint];
        Vector3 correctVector = idealVectors[currentPoint];

        if (isCoordinateDeviating(currentVector.x, correctVector.x) || isCoordinateDeviating(currentVector.y, correctVector.y) || isCoordinateDeviating(currentVector.z, correctVector.z))
        {
            changeIndicator(true);
        } else
        {
            changeIndicator(false);
        }
    }

    private bool isCoordinateDeviating(float currentCoordinate, float correctCoordinate)
    {
        if (currentCoordinate > 0)
        {
            float maxCoordinate = correctCoordinate + deviationRange;
            float minCoordinate = correctCoordinate - deviationRange;
            if (minCoordinate > currentCoordinate || currentCoordinate > maxCoordinate)
            {
                return true;
            } else
            {
                return false;
            }
        }

        if (currentCoordinate < 0)
        {
            float maxCoordinate = correctCoordinate - deviationRange;
            float minCoordinate = correctCoordinate + deviationRange;
            if (minCoordinate < currentCoordinate || currentCoordinate < maxCoordinate)
            {
                return true;
            } else
            {
                return false;
            }
        }

        return false;
    }

}