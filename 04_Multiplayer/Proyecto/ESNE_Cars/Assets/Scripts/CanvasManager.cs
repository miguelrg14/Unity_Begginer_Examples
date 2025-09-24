using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CircuitMng circuitMng;
    public CarController car;

    [Header("Canvas")]
    [SerializeField] GameObject summaryBoard_Canvas;
    public TMP_Text velocimeter;
    public TMP_Text maxCompetitors_txt;



    void FixedUpdate()
    {
        velocimeter.text = (car.rb.velocity.magnitude * 3.6).ToString("F0") + "Km/h";
    }

    public void Check_MaxCompetitors()
    {
        maxCompetitors_txt.text = circuitMng.competing.Count.ToString();
    }

    /// <summary>
    ///     Canvas - total race stats panel updater through:
    ///         - Competitors list
    ///         - Canvas stats block parent panel
    /// </summary>
    public void SetPlayerPositionPanel()
    {
        for (int i = 0; i < circuitMng.competing.Count; i++)
        {
            /// Name
            // If is a Player
            if (circuitMng.competing[i].GetComponent<Competitor_Info>() != null)
            {
                string onlineTag = circuitMng.competing[i].gameObject.GetComponent<Competitor_Info>().nickname;
                summaryBoard_Canvas.transform.GetChild(i).gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = onlineTag;
            }
            // If is ia
            else
            {
                summaryBoard_Canvas.transform.GetChild(i).gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = circuitMng.competing[i].name.ToString();
            }

            ///Position
            summaryBoard_Canvas.transform.GetChild(i).gameObject.transform.GetChild(1).GetComponent<TMP_Text>().text = circuitMng.competing[i].GetComponent<Competitor_Info>().position.ToString();

            // Declare winners / final positions
            // First finishing race
            if (circuitMng.competing.Count >= 1 && circuitMng.competing[0].GetComponent<Competitor_Info>().finishedRace)
            {
                summaryBoard_Canvas.transform.GetChild(0).gameObject.transform.GetChild(1).GetComponent<TMP_Text>().text = "Wins!";
            }
            // 2nd place
            if (circuitMng.competing.Count >= 2 && circuitMng.competing[1].GetComponent<Competitor_Info>().finishedRace)
            {
                summaryBoard_Canvas.transform.GetChild(1).gameObject.transform.GetChild(1).GetComponent<TMP_Text>().text = "2º Place";
            }
            // 3nd place
            if (circuitMng.competing.Count >= 3 && circuitMng.competing[2].GetComponent<Competitor_Info>().finishedRace)
            {
                summaryBoard_Canvas.transform.GetChild(2).gameObject.transform.GetChild(1).GetComponent<TMP_Text>().text = "3º Place";
            }
            // The rest when they finish
            if (circuitMng.competing.Count >= 4 && circuitMng.competing[3].GetComponent<Competitor_Info>().finishedRace)
            {
                summaryBoard_Canvas.transform.GetChild(3).gameObject.transform.GetChild(1).GetComponent<TMP_Text>().text = "4º Place";
            }
        }
    }
}
