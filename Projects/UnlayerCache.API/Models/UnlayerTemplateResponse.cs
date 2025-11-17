using System;

namespace UnlayerCache.API.Models
{
    public class UnlayerTemplateResponse
    {
        public bool success { get; set; }
        public Data data { get; set; }
    }

    public class UnlayerTemplateResponseMocked
    {
        public bool success { get; set; }
        public DataMocked data { get; set; }
    }

    public class DataMocked
    {
        public int id { get; set; }
        public string name { get; set; }
        public string design { get; set; }
        public string displayMode { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class Data
    {
        public int id { get; set; }
        public string name { get; set; }
        public Design design { get; set; }
        public string displayMode { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class Design
    {
        public Body body { get; set; }
        public Counters counters { get; set; }
        public int schemaVersion { get; set; }
    }

    public class Body
    {
        public string id { get; set; }
        public Row[] rows { get; set; }
        public Values values { get; set; }
    }

    public class Values
    {
        public _Meta _meta { get; set; }
        public Linkstyle linkStyle { get; set; }
        public string textColor { get; set; }
        public Fontfamily fontFamily { get; set; }
        public string popupWidth { get; set; }
        public string popupHeight { get; set; }
        public string borderRadius { get; set; }
        public string contentAlign { get; set; }
        public string contentWidth { get; set; }
        public string popupPosition { get; set; }
        public string preheaderText { get; set; }
        public string backgroundColor { get; set; }
        public Backgroundimage backgroundImage { get; set; }
        public string contentVerticalAlign { get; set; }
        public string popupBackgroundColor { get; set; }
        public Popupbackgroundimage popupBackgroundImage { get; set; }
        public Popupclosebutton_Action popupCloseButton_action { get; set; }
        public string popupCloseButton_margin { get; set; }
        public string popupCloseButton_position { get; set; }
        public string popupCloseButton_iconColor { get; set; }
        public string popupOverlay_backgroundColor { get; set; }
        public string popupCloseButton_borderRadius { get; set; }
        public string popupCloseButton_backgroundColor { get; set; }
    }

    public class _Meta
    {
        public string htmlID { get; set; }
        public string htmlClassNames { get; set; }
    }

    public class Linkstyle
    {
        public bool body { get; set; }
        public bool inherit { get; set; }
        public string linkColor { get; set; }
        public bool linkUnderline { get; set; }
        public string linkHoverColor { get; set; }
        public bool linkHoverUnderline { get; set; }
    }

    public class Fontfamily
    {
        public string url { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        public bool defaultFont { get; set; }
    }

    public class Backgroundimage
    {
        public string url { get; set; }
        public bool cover { get; set; }
        public bool center { get; set; }
        public bool repeat { get; set; }
        public bool fullWidth { get; set; }
    }

    public class Popupbackgroundimage
    {
        public string url { get; set; }
        public bool cover { get; set; }
        public bool center { get; set; }
        public bool repeat { get; set; }
        public bool fullWidth { get; set; }
    }

    public class Popupclosebutton_Action
    {
        public string name { get; set; }
        public Attrs attrs { get; set; }
    }

    public class Attrs
    {
        public string onClick { get; set; }
    }

    public class Row
    {
        public string id { get; set; }
        public int[] cells { get; set; }
        public Values1 values { get; set; }
        public Column[] columns { get; set; }
    }

    public class Values1
    {
        public _Meta1 _meta { get; set; }
        public string anchor { get; set; }
        public bool columns { get; set; }
        public string padding { get; set; }
        public bool hideable { get; set; }
        public bool deletable { get; set; }
        public bool draggable { get; set; }
        public bool selectable { get; set; }
        public bool hideDesktop { get; set; }
        public bool duplicatable { get; set; }
        public string backgroundColor { get; set; }
        public Backgroundimage1 backgroundImage { get; set; }
        public object displayCondition { get; set; }
        public string columnsBackgroundColor { get; set; }
    }

    public class _Meta1
    {
        public string htmlID { get; set; }
        public string htmlClassNames { get; set; }
    }

    public class Backgroundimage1
    {
        public string url { get; set; }
        public bool cover { get; set; }
        public bool center { get; set; }
        public bool repeat { get; set; }
        public bool fullWidth { get; set; }
    }

    public class Column
    {
        public string id { get; set; }
        public Values2 values { get; set; }
        public Content[] contents { get; set; }
    }

    public class Values2
    {
        public _Meta2 _meta { get; set; }
        public Border border { get; set; }
        public string padding { get; set; }
        public string backgroundColor { get; set; }
        public string borderRadius { get; set; }
    }

    public class _Meta2
    {
        public string htmlID { get; set; }
        public string htmlClassNames { get; set; }
    }

    public class Border
    {
    }

    public class Content
    {
        public string id { get; set; }
        public string type { get; set; }
        public Values3 values { get; set; }
    }

    public class Values3
    {
        public Src src { get; set; }
        public _Meta3 _meta { get; set; }
        public Action action { get; set; }
        public string anchor { get; set; }
        public string altText { get; set; }
        public bool hideable { get; set; }
        public bool deletable { get; set; }
        public bool draggable { get; set; }
        public string textAlign { get; set; }
        public bool selectable { get; set; }
        public bool hideDesktop { get; set; }
        public bool duplicatable { get; set; }
        public string containerPadding { get; set; }
        public object displayCondition { get; set; }
        public string html { get; set; }
        public _Override _override { get; set; }
    }

    public class Src
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class _Meta3
    {
        public string htmlID { get; set; }
        public string htmlClassNames { get; set; }
    }

    public class Action
    {
        public string name { get; set; }
        public Values4 values { get; set; }
    }

    public class Values4
    {
        public string href { get; set; }
        public string target { get; set; }
    }

    public class _Override
    {
        public Mobile mobile { get; set; }
        public Desktop desktop { get; set; }
    }

    public class Mobile
    {
        public bool hideMobile { get; set; }
    }

    public class Desktop
    {
        public bool hideDesktop { get; set; }
    }

    public class Counters
    {
        public int u_row { get; set; }
        public int u_column { get; set; }
        public int u_content_html { get; set; }
        public int u_content_image { get; set; }
    }

}
