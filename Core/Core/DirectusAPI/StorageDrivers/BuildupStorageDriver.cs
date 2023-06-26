﻿using System.Threading.Tasks;
using System.Collections.Generic;
using GraphQL;
using Calc.Core.Objects;
using Speckle.Newtonsoft.Json;

namespace Calc.Core.DirectusAPI.Drivers
{
    public class BuildupStorageDriver : IDriverGetMany<Buildup>
    {
        public string QueryGetMany { get; } = @"
                    query GetBuildups {
                      lca_buildups {
                        id
                        buildup_name
                        group_id {
                          group_name
                        }
                        components {
                          lca_materials_id {
                            id
                            material_name
                            global_warming_potential_a1_a2_a3
                            unit
                            material_category
                          }
                          amount
                        }
                        unit
                      }
                    }";

        [JsonProperty("lca_buildups")]
        public List<Buildup> GotManyItems { get; set; }

        public Dictionary<string, object> GetVariables()
        {
            return new Dictionary<string, object>();
        }
    }
}
