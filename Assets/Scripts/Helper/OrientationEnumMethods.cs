using UnityEngine;

public static class OrientationEnumMethods 
{
    public static Quaternion OrientationToQuaternion(this Orientation orientation)
    {
        Quaternion identity = Quaternion.identity;

        switch (orientation)
        {
            case Orientation.PositiveZ:
                identity.eulerAngles = new Vector3(0f, 0f, 0f);
                break;
            case Orientation.PositiveX:
                identity.eulerAngles = new Vector3(0f, 90f, 0f);
                break;
            case Orientation.NegativeZ:
                identity.eulerAngles = new Vector3(0f, 180f, 0f);
                break;
            case Orientation.NegativeX:
                identity.eulerAngles = new Vector3(0f, -90f, 0f);
                break;
        }
        return identity;
    }

    public static Orientation NextOrientation(this Orientation orientation, RotationSense rotationSense)
    {
        switch (rotationSense)
        {
            case RotationSense.CounterClockWise:
                switch (orientation)
                {
                    case Orientation.PositiveZ:
                        return Orientation.NegativeX;
                    case Orientation.NegativeX:
                        return Orientation.NegativeZ;
                    case Orientation.NegativeZ:
                        return Orientation.PositiveX;
                    case Orientation.PositiveX:
                        return Orientation.PositiveZ;
                }
                break;
            case RotationSense.ClockWise:
                switch (orientation)
                {
                    case Orientation.PositiveZ:
                        return Orientation.PositiveX;
                    case Orientation.PositiveX:
                        return Orientation.NegativeZ;
                    case Orientation.NegativeZ:
                        return Orientation.NegativeX;
                    case Orientation.NegativeX:
                        return Orientation.PositiveZ;
                }
                break;
        }
        return Orientation.Invalid;
    }

    public static Orientation OrientationFromTwoPositions(Vector3 position1, Vector3 position2)
    {

        Vector3 vector = position2 - position1;
        vector.y = 0f;

        float num = ((!(Vector3.Dot(vector, Vector3.right) > 0f)) ? (-1f) : 1f);
        float a = num * Vector3.Angle(vector, Vector3.forward);

        if (Mathf.Approximately(a, 0f))
        {
            return Orientation.PositiveZ;
        }
        if (Mathf.Approximately(a, 90f))
        {
            return Orientation.PositiveX;
        }
        if (Mathf.Approximately(a, 180f) || Mathf.Approximately(a, -180f))
        {
            return Orientation.NegativeZ;
        }
        if (Mathf.Approximately(a, -90f))
        {
            return Orientation.NegativeX;
        }
        return Orientation.Invalid;
    }

    public static Orientation ClosestOrientationFromTwoPositions(Vector3 Position1, Vector3 Position2)
    {
        Vector3 vector = Position2 - Position1;
        vector.y = 0f;

        float dot = ((!(Vector3.Dot(vector, Vector3.right) > 0f)) ? (-1f) : 1f);
        float angle = dot * Vector3.Angle(vector, Vector3.forward);

        if (angle > 180f)
        {
            return Orientation.Invalid;
        }
        if (angle > 135f)
        {
            return Orientation.NegativeZ;
        }
        if (angle > 45f)
        {
            return Orientation.PositiveX;
        }
        if (angle > -45f)
        {
            return Orientation.PositiveZ;
        }
        if (angle > -135f)
        {
            return Orientation.NegativeX;
        }
        if (angle > -180f)
        {
            return Orientation.NegativeZ;
        }
        return Orientation.Invalid;
    }

    public static Orientation OppositeOrientation(this Orientation a_Orientation)
    {
        switch (a_Orientation)
        {
            case Orientation.PositiveZ:
                return Orientation.NegativeZ;
            case Orientation.NegativeX:
                return Orientation.PositiveX;
            case Orientation.PositiveX:
                return Orientation.NegativeX;
            case Orientation.NegativeZ:
                return Orientation.PositiveZ;
            default:
                return Orientation.Invalid;
        }
    }
}
