using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using Scryfall.API.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Sammelkarten {

    public class WordDocument : ViewModelBase {

        #region Constructors

        public WordDocument() {
            InitializeOriginalDocument();
        }

        #endregion Constructors

        #region Events

        public event Action<WordDocument> OnFileFull;

        #endregion Events

        #region Properties

        public int PageCount { get; private set; }

        public XWPFDocument OriginalDocument { get; set; }

        public Bitmap Bitmap {
            get => _bitmap;
            private set { _bitmap = value; RaisePropertyChanged(); }
        }

        public long CountPictures { get => _countPictures; set { _countPictures = value; RaisePropertyChanged(); } }

        #endregion Properties

        #region Methods

        public void InitializeOriginalDocument() {
            OriginalDocument = new XWPFDocument();
            CurrentParagraph = OriginalDocument.CreateParagraph();

            //Seitenränder
            var SectPr = new CT_SectPr();
            SectPr.pgMar.left = 568;
            SectPr.pgMar.right = 568;
            SectPr.pgMar.top = "500";
            SectPr.pgMar.bottom = "500";
            OriginalDocument.Document.body.sectPr = SectPr;

            PageCount = 1;
            InitializePictures();
        }

        public async Task AddPictureTightAsync(IEnumerable<Card> cards, CancelToken cancelToken = null) {
            CountPictures = 0;
            foreach (var card in cards) {
                await AddPictureTightAsync(card);
                if (cancelToken?.IsCancelRequested ?? false) {
                    return;
                }
            }
        }

        public void SaveToFile(string path = "Cards.docx") {
            if (CountPictures % 9 > 0) {
                PrintToDoc();
            }
            var tempPath = path;

            //Prüfe auf fehlende Extension
            if (!System.IO.Path.HasExtension(tempPath)) {
                tempPath += ".docx";
            }
            var directory = Path.GetDirectoryName(tempPath);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }
            //Speicher Word Dokument in eine Datei
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                OriginalDocument.Write(stream);
            }
        }

        #endregion Methods

        #region Fields

        private XWPFParagraph CurrentParagraph;

        private long _countPictures;

        private Bitmap _bitmap;

        #endregion Fields

        private Graphics GanzesBild { get; set; }

        private async Task AddPictureTightAsync(Card card) {
            foreach (var face in card.PrintImages) {
                var imageStream = await App.NetClient.GetStreamAsync(face);
                var image = Image.FromStream(imageStream);
                for (var i = 0; i < card.Count; i++) {
                    PrintToBigPicture(image);
                    if (PageCount > 50) {
                        OnFileFull?.Invoke(this);
                        InitializeOriginalDocument();
                    }
                }
            }
        }

        private void InitializePictures() {
            Bitmap = new Bitmap(Card.CardWidthPixel * 3, Card.CardHeightPixel * 3);
            GanzesBild = Graphics.FromImage(Bitmap);
            GanzesBild.Clear(System.Drawing.Color.White);
        }

        private void PrintToBigPicture(Image image) {
            var (left, top) = GetNextLocation();
            GanzesBild.DrawImage(image, new RectangleF { X = left, Y = top, Height = Card.CardHeightPixel, Width = Card.CardWidthPixel });
            CountPictures++;

            //Neue Seite wenn Position ein vielfaches von 9
            if (CountPictures % 9 == 0) {
                //RaisePropertyChanged(nameof(Bitmap));
                PrintToDoc();
                CurrentParagraph.CreateRun().AddBreak();
                PageCount++;
            }
        }

        private void PrintToDoc() {
            using (var BitmapStream = new MemoryStream()) {
                Bitmap.Save(BitmapStream, ImageFormat.Jpeg);
                var CurrentRun = CurrentParagraph.CreateRun();
                BitmapStream.Position = 0;
                CurrentRun.AddPicture(BitmapStream, (int)PictureType.JPEG, "sample.jpg", Units.ToEMU(Card.CardWidth * 3), Units.ToEMU(Card.CardHeight * 3));
            }
            InitializePictures();
        }

        private (float left, float top) GetNextLocation() {
            float left = (CountPictures % 3) * Card.CardWidthPixel;
            float top = ((CountPictures % 9) / 3) * Card.CardHeightPixel;
            return (left, top);
        }

        //private static double PageHeight { get; } = 842;
        //private static double PageWidth { get; } = 595;

        //private static double MarginTopBottom { get; } = ((PageHeight - (3 * Card.CardHeight)) / 2);
        //private static double MarginLeftRight { get; } = ((PageWidth - (3 * Card.CardWidth)) / 2);

        //private CT_Anchor CreateAnchorWithGraphic(CT_GraphicalObject graphicalobject, string drawingDescr, int width,
        //                   int height, int left, int top) {
        //    var anchor = new CT_Anchor {
        //        //simplePos1 = false,
        //        relativeHeight = 2,
        //        behindDoc = false,
        //        distT = 0,
        //        distB = 0,
        //        distL = 0,
        //        distR = 0,
        //        locked = false,
        //        layoutInCell = true,
        //        allowOverlap = true,
        //        simplePos = new CT_Point2D {
        //            x = 0,
        //            y = 0
        //        },
        //        positionH = new CT_PosH {
        //            relativeFrom = ST_RelFromH.page,
        //            posOffset = left
        //        },
        //        positionV = new CT_PosV {
        //            relativeFrom = ST_RelFromV.page,
        //            posOffset = top
        //        },
        //        extent = new NPOI.OpenXmlFormats.Dml.WordProcessing.CT_PositiveSize2D {
        //            cx = width,
        //            cy = height
        //        },
        //        effectExtent = new CT_EffectExtent {
        //            l = 0,
        //            t = 0,
        //            r = 0,
        //            b = 0
        //        },
        //        wrapSquare = new CT_WrapSquare {
        //            wrapText = ST_WrapText.largest
        //        },
        //        //wrapTopAndBottom = new CT_WrapTopBottom(),
        //        //wrapTight = new CT_WrapTight {
        //        //    wrapText = ST_WrapText.bothSides,
        //        //    wrapPolygon = new CT_WrapPath {
        //        //        edited = false,
        //        //        start = new CT_Point2D {
        //        //            x = 0,
        //        //            y = 0
        //        //        },
        //        //        lineTo = new List<CT_Point2D> {
        //        //        new CT_Point2D { x = 0, y = 0 },
        //        //        new CT_Point2D { x = 21600, y = 21600 },
        //        //        new CT_Point2D { x = 21600, y = 0 },
        //        //        new CT_Point2D { x = 0, y = 0 }
        //        //    }
        //        //    }
        //        //},
        //        docPr = new NPOI.OpenXmlFormats.Dml.WordProcessing.CT_NonVisualDrawingProps {
        //            id = (uint)(CardRunCount + 1),
        //            name = "Bild" + (CardRunCount + 1),
        //            descr = $"{drawingDescr}"
        //        },
        //        cNvGraphicFramePr = new NPOI.OpenXmlFormats.Dml.WordProcessing.CT_NonVisualGraphicFrameProperties {
        //            graphicFrameLocks = new CT_GraphicalObjectFrameLocking {
        //                noChangeAspect = true
        //            }
        //        },
        //        graphic = graphicalobject
        //    };
        //    return anchor;
        //}
    }
}