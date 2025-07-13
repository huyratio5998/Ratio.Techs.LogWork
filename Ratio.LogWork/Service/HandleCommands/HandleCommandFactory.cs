using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleCommandFactory
    {
        private readonly IUnitOfWork _unitOfWork;

        public HandleCommandFactory(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IHandleCommands Create(WorkLogHistoryAction command)
        {
            IHandleCommands result = new HandleNoActionCommand();
            switch (command)
            {
                case WorkLogHistoryAction.Start:
                    result = new HandleStartCommand(_unitOfWork);
                    break;
                case WorkLogHistoryAction.Pause:
                    result = new HandlePauseCommand(_unitOfWork);
                    break;
                case WorkLogHistoryAction.Continue:
                    result = new HandleContinueCommand(_unitOfWork);
                    break;
                case WorkLogHistoryAction.Done:
                    result = new HandleDoneCommand(_unitOfWork);
                    break;
                case WorkLogHistoryAction.Cancel:
                    result = new HandleCancelCommand(_unitOfWork);
                    break;
                case WorkLogHistoryAction.NoAction:
                    result = new HandleNoActionCommand();
                    break;
            }

            return result;
        }
    }
}
