using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Repository;
using Ratio.LogWork.Service.HandleCommands.HandleNoActionCommand;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleCommandFactory
    {
        private readonly IUnitOfWork _unitOfWork;

        public HandleCommandFactory(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IHandleCommands Create(WorkLogHistoryAction historyAction, string command)
        {
            IHandleCommands result = new HandleDoNothingCommand(_unitOfWork);
            switch (historyAction)
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
                    {
                        if (command.Equals(WorkLogHelper.SHOW, StringComparison.OrdinalIgnoreCase))
                            result = new HandleShowCommand(_unitOfWork);
                        else if (command.Equals(WorkLogHelper.REPORT, StringComparison.OrdinalIgnoreCase))
                            result = new HandleReportCommand(_unitOfWork);
                        else if (command.Equals(WorkLogHelper.EDIT, StringComparison.OrdinalIgnoreCase))
                            result = new HandleReportCommand(_unitOfWork);
                        else
                            result = new HandleDoNothingCommand(_unitOfWork);
                        break;
                    }
            }

            return result;
        }
    }
}
