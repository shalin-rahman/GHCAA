export class ExportUtil {
  /**
   * Export JSON data to Excel (.xlsx)
   */
  static toExcel(data: any[], fileName: string = 'export'): void {
    if (!data || data.length === 0) { console.warn('ExportUtil: No data to export'); return; }
    import('xlsx').then(XLSX => {
      const ws = XLSX.utils.json_to_sheet(data);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Data');
      const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
      ExportUtil.saveFile(
        new Blob([wbout], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }),
        `${fileName}.xlsx`
      );
    }).catch(err => console.error('Excel export failed:', err));
  }

  /**
   * Export JSON data to CSV
   */
  static toCsv(data: any[], fileName: string = 'export'): void {
    if (!data || data.length === 0) { console.warn('ExportUtil: No data to export'); return; }

    const headers = Object.keys(data[0]);
    const csvRows = [
      headers.join(','),
      ...data.map(row => headers.map(header => {
        const value = row[header];
        const valStr = value === null || value === undefined ? '' : String(value);
        return valStr.includes(',') || valStr.includes('"') || valStr.includes('\n')
          ? `"${valStr.replace(/"/g, '""')}"` : valStr;
      }).join(','))
    ];

    const csvContent = '\uFEFF' + csvRows.join('\n');
    ExportUtil.saveFile(
      new Blob([csvContent], { type: 'text/csv;charset=utf-8;' }),
      `${fileName}.csv`
    );
  }

  /**
   * Export data to PDF using jspdf-autotable
   */
  static async toPdf(headers: string[], data: any[][], fileName: string = 'export', title: string = 'Data Export'): Promise<void> {
    try {
      const jspdfModule = await import('jspdf');
      const jsPDF = jspdfModule.jsPDF || jspdfModule.default;
      const autoTableModule = await import('jspdf-autotable');
      const autoTable = autoTableModule.default || autoTableModule;

      const doc = new jsPDF();
      doc.setFontSize(18);
      doc.text(title, 14, 22);
      doc.setFontSize(10);
      doc.text(`Generated on: ${new Date().toLocaleString()}`, 14, 30);

      autoTable(doc as any, {
        head: [headers],
        body: data,
        startY: 35,
        theme: 'grid',
        styles: { fontSize: 8 },
        headStyles: { fillColor: [22, 101, 52] },
      });

      doc.save(`${fileName}.pdf`);
    } catch (err) {
      console.error('PDF export failed:', err);
    }
  }

  private static saveFile(blob: Blob, filename: string): void {
    const nav = window.navigator as any;
    if (nav.msSaveOrOpenBlob) {
      nav.msSaveOrOpenBlob(blob, filename);
      return;
    }
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    
    // CRITICAL: Delay before revoking the object URL and removing the element.
    // If done synchronously, the browser aborts the download resulting in a corrupted/0-byte file.
    setTimeout(() => {
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    }, 250);
  }
}
