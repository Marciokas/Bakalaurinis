using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;

public class PathFollower : MonoBehaviour
{
    private Vector3[] realVectors = new Vector3[66];
    private Vector3[] idealVectors = new Vector3[66];
    private Vector3 _direction;
    private Quaternion _lookRotation;
    public float speed = 50.0f;
    public float reachDist = 1.0f;
    public float rotationSpeed = 1.0f;
    public float D5Deviation = 5.0f;
    public float D4Deviation = 4.0f;
    public float D3Deviation = 3.0f;
    public float D2Deviation = 2.0f;
    public float D1Deviation = 1.0f;
    public float D4Alt = 17.6f;
    public float D3Alt = 13.9f;
    public float D2Alt = 10.15f;
    public float D1Alt = 6.45f;
    private float dist;
    public int currentPoint = 0;
    public Button buttonX;
    public Button buttonY;
    public Button buttonZ;
    public GameObject prefab;
    private static double Q = 0.000001;
    private static double R = 0.000004;
    private static double P = 1, X = 0, K;

    void Start()
    {
        //Failo nuskaitymas
		StreamReader inp_stm = new StreamReader(Application.dataPath + "\\realTrajectory.txt");

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
		inp_stm = new StreamReader(Application.dataPath + "\\idealTrajectory.txt");

        i = 0;
        //Ciklas vykdomas iki nuskaityto failo pabaigos
        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            String[] vectorArray = inp_ln.Split(' ');
            //Kiekvienai koordinatei sukuriamas Vector3 objektas ir kiekviena koordinatė yra išplėčiama atitinkamai koordinačių kiekiui (-i*10)
            Vector3 vector = new Vector3(float.Parse(vectorArray[0]) - i * 10, float.Parse(vectorArray[2]), float.Parse(vectorArray[1]) - i * 10);
            //Ši koordinatė patalpinama į sąrašą
            idealVectors[i] = vector;  
            i++;
        }
       generateIdealTrajectory();

        //Uždaromas nuskaitytas failas
        inp_stm.Close();
    }

    void generateIdealTrajectory()
    {
        float deviationRange = D5Deviation;
        for (int i = 0; i < idealVectors.Length - 1; i++)
        {
            //Nustatomas atstumas nuo esamos iki sekančios koordinatės
            dist = Vector3.Distance(idealVectors[i], idealVectors[i+1]);
            //Surandama kryptis nuo esamos koordinatės iki sekančios
            _direction = (idealVectors[i] - idealVectors[i + 1]).normalized;
            if (_direction != Vector3.zero)
            {
                _lookRotation = Quaternion.LookRotation(_direction);
                //Inicijuojamas naujas apskritimas
                GameObject circle = Instantiate(prefab, idealVectors[i], Quaternion.Euler(new Vector3(_lookRotation.x, -40, 90))) as GameObject;
                deviationRange = processDeviationRange(i);

                //Nustatomas apskritimo skersmuo
                circle.transform.localScale = Vector3.one * (deviationRange/10);
            }            
        }
    }

    float processDeviationRange(int i)
    {
        if (idealVectors[i].y < D4Alt && idealVectors[i].y > D3Alt)
        {
            return D4Deviation;
        }
        if (idealVectors[i].y < D3Alt && idealVectors[i].y > D2Alt)
        {
            return D3Deviation;
        }
        if (idealVectors[i].y < D2Alt && idealVectors[i].y > D1Alt)
        {
            return D2Deviation;
        }
        if (idealVectors[i].y < D1Alt)
        {
            return D1Deviation;
        }
        return D5Deviation;
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

    private void changeIndicator(bool dangerActive, String coordinate)
    {
        Color color = dangerActive ? Color.red : Color.green;

        switch (coordinate)
        {
            case "X":
                buttonX.image.color = color;
                break;
            case "Y":
                buttonY.image.color = color;
                break;
            case "Z":
                buttonZ.image.color = color;
                break;
            default:
                throw new Exception();
        }
    }

    private void checkVectorDeviation()
    {
        Vector3 currentVector = realVectors[currentPoint];
        Vector3 correctVector = idealVectors[currentPoint];
        float deviationRange = processDeviationRange(currentPoint);

        // Tikrinamos visos koordinatės iš eilės X, Y, Z ir jeigu bent viena neatitinka rėžių, keičiama indikatoriaus spalva
        changeIndicator(isCoordinateDeviating(currentVector.x, correctVector.x, deviationRange), "X");
        changeIndicator(isCoordinateDeviating(currentVector.y, correctVector.y, deviationRange), "Y");
        changeIndicator(isCoordinateDeviating(currentVector.z, correctVector.z, deviationRange), "Z");
    }

    //Koordinačių paklaidos matavimo metodas
    private bool isCoordinateDeviating(float currentCoordinate, float correctCoordinate, float deviationRange)
    {
        if (currentCoordinate > 0)
        {
            //Apskaičiuojami teigiamos koordinatės rėžiai
            float maxCoordinate = correctCoordinate + deviationRange;
            float minCoordinate = correctCoordinate - deviationRange;
           
            return minCoordinate > currentCoordinate || currentCoordinate > maxCoordinate;
        }

        if (currentCoordinate < 0)
        {
            //Apskaičiuojami neigiamos koordinatės rėžiai
            float maxCoordinate = correctCoordinate - deviationRange;
            float minCoordinate = correctCoordinate + deviationRange;
            
            return minCoordinate < currentCoordinate || currentCoordinate < maxCoordinate;
        }

        return false;
    }

    /*void OnDrawGizmos()
    {
        for (int i = 0; i < idealVectors.Length - 1; i++)
        {
            Handles.color = Color.black;
            Handles.DrawWireDisc(idealVectors[i], new Vector3(1f, 1f, 4f), processDeviationRange(currentPoint));
        }
    }*/

}