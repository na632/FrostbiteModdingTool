using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.ModsAndProjects.Projects
{
    internal class FMTProject
    {
        public FMTProject() 
        { 

        }

        public static FMTProject Create()
        {
            FMTProject project = new FMTProject();// this;
            return project;
        }

        public static FMTProject Read(string filePath)
        {
            if(!File.Exists(filePath))
                throw new FileNotFoundException(filePath);



            FMTProject project = new FMTProject();
            return project;
        }

        public FMTProject Update()
        {
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
