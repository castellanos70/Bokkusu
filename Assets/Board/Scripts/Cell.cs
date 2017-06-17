using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private CameraScript.Element environment;
    private CameraScript.Element entity;
    private GameObject environmentObj;

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

    public void setEnvironmentObj(GameObject obj)
    {
        this.environmentObj = obj;
    }

    public bool setEntity(CameraScript.Element entity, bool prejudice)
    {
        if (this.entity == CameraScript.Element.NOTHING || prejudice)
        {
            this.entity = entity;
            return true;
        }
        else return false;
    }

    public bool removeEntity(CameraScript.Element remover)
    {
        if (entity != remover && entity != CameraScript.Element.NOTHING) return false;
        entity = CameraScript.Element.NOTHING;
        return true;
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

    public void destroyEnvironment()
    {
        if (environmentObj != null) Object.Destroy(environmentObj);
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
