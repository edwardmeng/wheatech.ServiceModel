﻿using System;
using Wheatech.ServiceModel.Sample.Components;

namespace Wheatech.ServiceModel.Samples.WebForm
{
    public partial class Default : System.Web.UI.Page
    {
        [Injection]
        public ICacheRepository Repository { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            textCacheValue.Text = Convert.ToString(Repository.GetVale("Sample"));
        }
    }
}