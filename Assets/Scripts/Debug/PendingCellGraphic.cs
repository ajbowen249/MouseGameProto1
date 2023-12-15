using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PendingCellGraphic : MonoBehaviour
{
    public TMP_Text NorthCar;
    public TMP_Text SouthCar;
    public TMP_Text EastCar;
    public TMP_Text WestCar;

    public TMP_Text NorthFoot;
    public TMP_Text SouthFoot;
    public TMP_Text EastFoot;
    public TMP_Text WestFoot;

    public TMP_Text OptionCount;

    public void SetOptions(PendingCell<WFCCell> cell)
    {
        var map = new Dictionary<AttachModeType, Dictionary<AttachEdge, TMP_Text>>
        {
            {
                AttachModeType.CAR,
                new Dictionary<AttachEdge, TMP_Text>
                {
                    { AttachEdge.NORTH, NorthCar },
                    { AttachEdge.SOUTH, SouthCar },
                    { AttachEdge.EAST, EastCar },
                    { AttachEdge.WEST, WestCar },
                }
            },
            {
                AttachModeType.FOOT,
                new Dictionary<AttachEdge, TMP_Text>
                {
                    { AttachEdge.NORTH, NorthFoot },
                    { AttachEdge.SOUTH, SouthFoot },
                    { AttachEdge.EAST, EastFoot },
                    { AttachEdge.WEST, WestFoot },
                }
            },
        };

        OptionCount.text = $"{cell.PossibleCells.Count}";

        var mustConnectColor = Color.green;
        var mayConnectColor = Color.magenta;

        foreach (var mode in map.Keys)
        {
            var modeOptions = cell.PossibleCells.Where(option => option.AttachPoints.Any(point => point.modeType == mode));

            foreach (var edge in map[mode].Keys)
            {
                var mayConnect = modeOptions.Any(option => option.AttachPoints.Any(point => point.modeType == mode && point.edge == edge));
                var mustConnect = modeOptions.All(option => option.AttachPoints.Any(point => point.modeType == mode && point.edge == edge));

                if (mayConnect)
                {
                    map[mode][edge].gameObject.SetActive(true);
                    map[mode][edge].color = mustConnect ? mustConnectColor : mayConnectColor;
                }
                else
                {
                    map[mode][edge].gameObject.SetActive(false);
                }
            }
        }
    }
}
