using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Prism.Commands;
using RevitAPILabelingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPILabelingApplication
{
    public class MainViewViewModel
    {

        private ExternalCommandData _commandData;


        public List<FamilySymbol> Tags { get; }

        public Pipe Pipe { get; }

        public FamilySymbol SelectedTagType { get; set; }

        public DelegateCommand SaveCommand { get; }



        public MainViewViewModel(ExternalCommandData commandData) //конструктор
        {
            _commandData = commandData;
            Tags = TagsUtils.GetPipeTagTypes(commandData);//загружаем в список всех меток
            Pipe = SelectionUtils.GetObject<Pipe>(_commandData, "Выберите трубу");
            SaveCommand = new DelegateCommand(OnSaveCommand);

        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var pipeLocCurve = Pipe.Location as LocationCurve;//извлекаем LocationCurve из трубы
            var pipeCurve = pipeLocCurve.Curve;//дополнительно преобразуем в Curve для того, чтобы получить ось трубы
            var pipeMidPoint = (pipeCurve.GetEndPoint(0) + pipeCurve.GetEndPoint(1)) / 2;//узнаем среднюю точку трубы для того чтобы нам втавить метку правильно

            using (var ts = new Transaction(doc, "Create tag")) //создаем новую метку
            {
                ts.Start();
                IndependentTag.Create(doc, SelectedTagType.Id, doc.ActiveView.Id, new Reference(Pipe), false, TagOrientation.Horizontal, pipeMidPoint);

                ts.Commit();
            }

            RaiseCloseRequest();//закрытие окна

        }

        public event EventHandler CloseRequest;

        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

       

        

    }
}
