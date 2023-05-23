using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class DAL_GameObject : MonoBehaviour
    {
        void Update()
        {
            if (this.gameObject.GetComponent<ParticleSystem>()?.isPaused == false && Find.TickManager.Paused)
            {
                this.gameObject.GetComponent<ParticleSystem>().Pause();
            }
            if (this.gameObject.GetComponent<ParticleSystem>()?.isPaused == true && !Find.TickManager.Paused)
            {
                this.gameObject.GetComponent<ParticleSystem>().Play();
            }
        }
    }
}
