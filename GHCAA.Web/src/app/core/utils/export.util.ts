import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

export class ExportUtil {
  /**
   * Export JSON data to Excel (.xlsx)
   */
  static toExcel(data: any[], fileName: string = 'export'): void {
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(data);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Data');
    XLSX.writeFile(wb, `${fileName}.xlsx`);
  }

  /**
   * Export JSON data to CSV
   */
  static toCsv(data: any[], fileName: string = 'export'): void {
    if (data.length === 0) return;
    
    const headers = Object.keys(data[0]);
    const csvRows = [
      headers.join(','), // header row
      ...data.map(row => headers.map(header => {
        const value = row[header];
        const valStr = value === null || value === undefined ? '' : String(value);
        // Escape quotes and wrap in quotes if contains comma
        return valStr.includes(',') ? `"${valStr.replace(/"/g, '""')}"` : valStr;
      }).join(','))
    ];
    
    const csvContent = csvRows.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `${fileName}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  /**
   * Export data to PDF using jspdf-autotable
   */
  static toPdf(headers: string[], data: any[][], fileName: string = 'export', title: string = 'Data Export'): void {
    const doc = new jsPDF();
    doc.setFontSize(18);
    doc.text(title, 14, 22);
    doc.setFontSize(10);
    doc.text(`Generated on: ${new Date().toLocaleString()}`, 14, 30);
    
    autoTable(doc, {
      head: [headers],
      body: data,
      startY: 35,
      theme: 'grid',
      styles: { fontSize: 8 },
      headStyles: { fillColor: [22, 101, 52] }, // GHC Green
    });
    
    doc.save(`${fileName}.pdf`);
  }
}
