using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceMatrix : MonoBehaviour
{

    // list of references to the units of the army 0
    public static List<UnitBase> Army0 = new List<UnitBase>();

    // list of references to the units of the army 1
    public static List<UnitBase> Army1 = new List<UnitBase>();

    // rows = Army0 | columns = Army1
    // distanceMatrix[2][3] is the distance^2 between the Army0[2] and the Army1[3] units
    public static List<List<float>> distanceMatrix = new List<List<float>>();

    private enum IterateMode
    {
        no_search,
        all_at_once,
        mini_matrix
    }
    private IterateMode mode = IterateMode.all_at_once;

    private void LateUpdate()
    {
        if (Army0.Count == 0 || Army1.Count == 0) return;

        switch (mode)
        {
            case IterateMode.no_search:
                break;
            case IterateMode.all_at_once:
                Update_All();
                break;
            case IterateMode.mini_matrix:
                break;
        }
    }

    private void Update_All()
    {
        for (int i = 0; i < Army0.Count; i++)
        {
            for (int j = 0; j < Army1.Count; j++)
            {
                Update_Distance(i, j);
            }
        }
    }
    private void Update_Distance(int i, int j)
    {
        UnitBase unit0 = Army0[i];
        UnitBase unit1 = Army1[j];

        float prevDist = distanceMatrix[i][j];

        float xDist = Mathf.Abs(unit0.transform.position.x - unit1.transform.position.x);

        if (xDist < unit0.visionSphereRadius || xDist < unit1.visionSphereRadius)
        {
            float zDist = Mathf.Abs(unit0.transform.position.z - unit1.transform.position.z);

            if (zDist < unit0.visionSphereRadius || zDist < unit1.visionSphereRadius)
            {
                float newDist = xDist * xDist + zDist * zDist;
                distanceMatrix[i][j] = newDist;

                if (prevDist >= unit0.visionSphereRadius2 && newDist <= unit0.visionSphereRadius2)
                {
                    // unit0 sees unit1 for the first time
                    unit0.Enemy_Enters_VisionSphere(unit1);
                }
                else if (prevDist < unit0.visionSphereRadius2 && newDist > unit0.visionSphereRadius2)
                {
                    // unit0 stops seeing unit1
                    unit0.Enemy_Leaves_VisionSphere(unit1);
                }

                if (prevDist >= unit1.visionSphereRadius2 && newDist <= unit1.visionSphereRadius2)
                {
                    // unit1 sees unit0 for the first time
                    unit1.Enemy_Enters_VisionSphere(unit0);
                }
                else if (prevDist < unit1.visionSphereRadius2 && newDist > unit1.visionSphereRadius2)
                {
                    // unit1 stops seeing unit0
                    unit1.Enemy_Leaves_VisionSphere(unit0);
                }
            }
            else
            {
                // distance on z is bigger thatn the sphere radius
                if (prevDist <= unit0.visionSphereRadius2)
                {
                    // unit0 stops seeing unit1
                    unit0.Enemy_Leaves_VisionSphere(unit1);
                }
                if (prevDist <= unit1.visionSphereRadius2)
                {
                    // unit1 stops seeing unit1
                    unit1.Enemy_Leaves_VisionSphere(unit0);
                }

                distanceMatrix[i][j] = float.MaxValue;
            }
        }
        else
        {
            // distance on x is bigger than the sphere radius
            if (prevDist <= unit0.visionSphereRadius2)
            {
                // unit0 stops seeing unit1
                unit0.Enemy_Leaves_VisionSphere(unit1);
            }
            if (prevDist <= unit1.visionSphereRadius2)
            {
                // unit1 stops seeing unit1
                unit1.Enemy_Leaves_VisionSphere(unit0);
            }

            distanceMatrix[i][j] = float.MaxValue;
        }
    }

    public static void Insert_Unit(UnitBase unit)
    {
        if (unit.team.teamNumber == 0)
        {
            // insert a new row in the matrix
            Army0.Add(unit);

            List<float> distanceList = new List<float>();
            for (int i = 0; i < Army1.Count; i++)
                distanceList.Add(float.MaxValue);

            distanceMatrix.Add(distanceList);
        }
        else if (unit.team.teamNumber == 1)
        {
            // insert a new column in the matrix
            Army1.Add(unit);

            for (int i = 0; i < Army0.Count; i++)
                distanceMatrix[i].Add(float.MaxValue);
        }
    }
    public static void Remove_Unit(UnitBase unit)
    {
        if (unit.team.teamNumber == 0)
        {
            // remove a row
            int unitIndex = Army0.IndexOf(unit);

            // remove this unit from all the enemies that are seing it
            for (int i = 0; i < Army1.Count; i++)
            {
                if (distanceMatrix[unitIndex][i] <= Army1[i].visionSphereRadius2)
                    Army1[i].Enemy_Leaves_VisionSphere(unit);
            }

            Army0.RemoveAt(unitIndex);
            distanceMatrix.RemoveAt(unitIndex);
        }
        else if (unit.team.teamNumber == 1)
        {
            // remove a column
            int unitIndex = Army1.IndexOf(unit);

            // remove this unit from all the enemies that are seing it
            for (int i = 0; i < Army0.Count; i++)
            {
                if (distanceMatrix[i][unitIndex] <= Army0[i].visionSphereRadius2)
                    Army0[i].Enemy_Leaves_VisionSphere(unit);
            }

            Army1.RemoveAt(unitIndex);
            for (int i = 0; i < Army0.Count; i++)
                distanceMatrix[i].RemoveAt(unitIndex);
        }
    }

    private void OnGUI()
    {
        GUI.skin.label.fontSize = 10;

        int iCount = distanceMatrix.Count;
        int jCount;
        for (int i = 0; i < iCount; i++)
        {
            jCount = distanceMatrix[i].Count;
            for (int j = 0; j < jCount; j++)
            {
                float aux = distanceMatrix[i][j];
                if (aux != float.MaxValue)
                    GUI.Label(new Rect(100 + 50 * j, 100 + 20 * i, 50, 20), aux.ToString());
                else
                    GUI.Label(new Rect(100 + 50 * j, 100 + 20 * i, 50, 20), "---");
            }
        }
    }

}
