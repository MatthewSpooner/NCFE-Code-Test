using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncfe.CodeTest
{
    public class LearnerService
    {
        public Learner GetLearner(int learnerId, bool isLearnerArchived)
        {
            Learner archivedLearner = null;

            if (isLearnerArchived)
            {
                var archivedDataService = new ArchivedDataService();
                archivedLearner = archivedDataService.GetArchivedLearner(learnerId);

                return archivedLearner;
            }
            else
            {

                var failoverRespository = new FailoverRepository();
                var failoverEntries = failoverRespository.GetFailOverEntries();


                var failedRequests = 0;

                foreach (var failoverEntry in failoverEntries)
                {
                    if (failoverEntry.DateTime > DateTime.Now.AddMinutes(-10))
                    {
                        failedRequests++;
                    }
                }

                LearnerResponse learnerResponse = null;
                Learner learner = null;

                if (failedRequests > 100 && (ConfigurationManager.AppSettings["IsFailoverModeEnabled"] == "true" || ConfigurationManager.AppSettings["IsFailoverModeEnabled"] == "True"))
                {
                    learnerResponse = FailoverLearnerDataAccess.GetLearnerById(learnerId);
                }
                else
                {
                    var dataAccess = new LearnerDataAccess();
                    learnerResponse = dataAccess.LoadLearner(learnerId);


                }

                if (learnerResponse.IsArchived)
                {
                    var archivedDataService = new ArchivedDataService();
                    learner = archivedDataService.GetArchivedLearner(learnerId);
                }
                else
                {
                    learner = learnerResponse.Learner;
                }


                return learner;
            }
        }

    }
}
