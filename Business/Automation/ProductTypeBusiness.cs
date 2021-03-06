﻿using Models.Generated.AutomationDB.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Automation
{
    public class ProductTypeBusiness : AutomationBaseBusiness<ProductType>
    {
        public ProductType GetByID(long productid)
        {
            var q = this.GetAll(1);
            q.And(ProductType.Columns.ID, productid);

            return this.Fetch(q).FirstOrDefault();
        }

        public List<ProductType> GetByProductClassificationID(long classificationID)
        {
            var q = this.GetAll(300);
            if (classificationID != 0)
                q.And(ProductType.Columns.ProductClassificationID, classificationID);
            q.OrderBy(ProductType.Columns.ID, "DESC");
            return this.Fetch(q).ToList();
        }
    }
}
