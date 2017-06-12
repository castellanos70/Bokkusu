using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private CameraScript.Element environment;
    private CameraScript.Element entity;

    public Cell(CameraScript.Element environment, CameraScript.Element entity)
    {
        this.environment = environment;
        this.entity = entity;
    }

    public Cell(CameraScript.Element environment)
    {
        this.environment = environment;
        entity = CameraScript.Element.NOTHING;
    }

    public bool setEntity(CameraScript.Element entity)
    {
        if (this.entity == CameraScript.Element.NOTHING)
        {
            this.entity = entity;
            return true;
        }
        else return false;
    }

    public void removeEntity()
    {
        entity = CameraScript.Element.NOTHING;
    }

    public CameraScript.Element getEnvironment()
    {
        return environment;
    }

    public bool hasEnvironment(CameraScript.Element environment)
    {
        if (this.environment == environment) return true;
        return false;
    }

    public CameraScript.Element getEntity()
    {
        return entity;
    }

    public bool hasEntity(CameraScript.Element entity)
    {
        if (this.entity == entity) return true;
        return false;
    }

}
