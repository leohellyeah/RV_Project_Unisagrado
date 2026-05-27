"""Converte o artigo SBC de Markdown para .docx, removendo os placeholders de figura."""
import re
from pathlib import Path
from docx import Document
from docx.shared import Pt, Inches, RGBColor

BASE = Path(__file__).parent
MD_PATH = BASE / "Artigo_SBC_VR_Folklore.md"
DOCX_PATH = BASE / "Artigo_SBC_VR_Folklore.docx"


def parse_inline(text):
    """Quebra texto em runs com formatacao: (texto, bold, italic, is_code)."""
    parts = []
    pattern = re.compile(r"(\*\*[^*]+\*\*|\*[^*]+\*|`[^`]+`)")
    pos = 0
    for m in pattern.finditer(text):
        if m.start() > pos:
            parts.append((text[pos:m.start()], False, False, False))
        chunk = m.group(0)
        if chunk.startswith("**"):
            parts.append((chunk[2:-2], True, False, False))
        elif chunk.startswith("`"):
            parts.append((chunk[1:-1], False, False, True))
        else:
            parts.append((chunk[1:-1], False, True, False))
        pos = m.end()
    if pos < len(text):
        parts.append((text[pos:], False, False, False))
    return parts


def add_paragraph(doc, text, style=None):
    p = doc.add_paragraph(style=style) if style else doc.add_paragraph()
    for txt, bold, italic, is_code in parse_inline(text):
        run = p.add_run(txt)
        run.bold = bold
        run.italic = italic
        if is_code:
            run.font.name = "Consolas"
            run.font.size = Pt(10)
    return p


def filter_lines(raw):
    """Remove blockquotes de placeholder de figura. Mantem tabelas e remove apenas o aviso."""
    out = []
    lines = raw.split("\n")
    i = 0
    while i < len(lines):
        line = lines[i]
        stripped = line.strip()

        # Bloco placeholder de FIGURA: descarta o blockquote inteiro ate proximo "vazio"
        if stripped.startswith("> **[FIGURA"):
            while i < len(lines) and lines[i].strip().startswith(">"):
                i += 1
            # consome eventual linha em branco que segue
            if i < len(lines) and lines[i].strip() == "":
                i += 1
            continue

        # Bloco placeholder de TABELA: descarta apenas o aviso, mantem a tabela
        if stripped.startswith("> **[TABELA"):
            # pula a linha do aviso
            i += 1
            # pula linhas '>' vazias antes da tabela
            while i < len(lines) and lines[i].strip() in (">", ""):
                i += 1
            # extrai linhas da tabela (que comecam com '> |')
            while i < len(lines) and lines[i].strip().startswith("> |"):
                out.append(lines[i].strip()[2:])  # remove o '> ' do inicio
                i += 1
            continue

        out.append(line)
        i += 1
    return "\n".join(out)


def md_to_docx():
    raw = MD_PATH.read_text(encoding="utf-8")
    raw = filter_lines(raw)

    doc = Document()
    style = doc.styles["Normal"]
    style.font.name = "Calibri"
    style.font.size = Pt(11)

    lines = raw.split("\n")
    i = 0
    in_code = False
    code_buf = []
    table_buf = []

    def flush_table():
        nonlocal table_buf
        if not table_buf:
            return
        headers = [c.strip() for c in table_buf[0].strip("|").split("|")]
        data = []
        for row in table_buf[2:]:  # pula separador
            cells = [c.strip() for c in row.strip("|").split("|")]
            # pad ou trunca pra bater com headers
            cells = (cells + [""] * len(headers))[:len(headers)]
            data.append(cells)
        table = doc.add_table(rows=1 + len(data), cols=len(headers))
        try:
            table.style = "Light Grid Accent 1"
        except KeyError:
            pass
        for idx, h in enumerate(headers):
            cell = table.rows[0].cells[idx]
            cell.text = ""
            p = cell.paragraphs[0]
            run = p.add_run(h)
            run.bold = True
        for r_idx, row in enumerate(data):
            for c_idx, txt in enumerate(row):
                cell = table.rows[r_idx + 1].cells[c_idx]
                cell.text = ""
                p = cell.paragraphs[0]
                for chunk, b, it, code in parse_inline(txt):
                    run = p.add_run(chunk)
                    run.bold = b
                    run.italic = it
                    if code:
                        run.font.name = "Consolas"
                        run.font.size = Pt(10)
        doc.add_paragraph()
        table_buf = []

    while i < len(lines):
        line = lines[i]
        stripped = line.strip()

        if stripped.startswith("```"):
            if in_code:
                p = doc.add_paragraph()
                run = p.add_run("\n".join(code_buf))
                run.font.name = "Consolas"
                run.font.size = Pt(10)
                code_buf = []
                in_code = False
            else:
                in_code = True
            i += 1
            continue

        if in_code:
            code_buf.append(line)
            i += 1
            continue

        # Tabela
        if stripped.startswith("|") and stripped.endswith("|"):
            table_buf.append(stripped)
            i += 1
            continue
        if table_buf and not (stripped.startswith("|") and stripped.endswith("|")):
            flush_table()

        # Cabecalhos
        if stripped.startswith("# "):
            doc.add_heading(stripped[2:].strip(), level=0)
        elif stripped.startswith("## "):
            doc.add_heading(stripped[3:].strip(), level=1)
        elif stripped.startswith("### "):
            doc.add_heading(stripped[4:].strip(), level=2)
        elif stripped.startswith("#### "):
            doc.add_heading(stripped[5:].strip(), level=3)
        elif stripped.startswith("- "):
            add_paragraph(doc, stripped[2:].strip(), style="List Bullet")
        elif re.match(r"^\d+\.\s", stripped):
            txt = re.sub(r"^\d+\.\s", "", stripped).strip()
            add_paragraph(doc, txt, style="List Number")
        elif stripped == "---":
            pass  # separador silencioso
        elif stripped.startswith("> "):
            p = add_paragraph(doc, stripped[2:].strip())
            p.paragraph_format.left_indent = Inches(0.4)
            for run in p.runs:
                run.font.color.rgb = RGBColor(0x55, 0x55, 0x55)
        elif stripped == "":
            pass
        else:
            add_paragraph(doc, stripped)

        i += 1

    flush_table()
    doc.save(DOCX_PATH)
    print(f"OK -> {DOCX_PATH}")


if __name__ == "__main__":
    md_to_docx()
