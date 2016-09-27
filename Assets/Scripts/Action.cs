using System.Collections.Generic;

public class Action {

    private static Graph graph = MainController.Graph;

    private Action()
    {
    }

    public static Action Execute()
    {
        Action action = new Action();

        for (int i = 0; i < graph.Units.Count; i++)
        {
            Unit unit = graph.Units[i];

            if (unit is Player)
                continue;

            List<Distance> distances = graph.GetDistancesForUnit(unit);
            distances.Sort();

            foreach (Distance distance in distances)
            {
                if (distance.Length == 2)
                {
                    if (unit.CanSkipAction)
                        unit.CanSkipAction = false;
                    else
                    {
                        unit.Move(distance[1]);
                        unit.CanSkipAction = true;
                    }
                    break;
                }

                if (distance.Length == 1)
                {
                    unit.Move(distance.To);
                    break;
                }

                unit.Move(distance[1]);
                break;
            }

            unit++;
        }

        return action;
    }
}
