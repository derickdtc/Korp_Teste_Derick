import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ItemNotaFiscal, NotaFiscal, Produto } from './models';

@Injectable({ providedIn: 'root' })
export class NfeApiService {
  private readonly http = inject(HttpClient);
  private readonly estoqueUrl = 'http://localhost:5115/api/produtos';
  private readonly faturamentoUrl = 'http://localhost:5116/api/notas-fiscais';

  listarProdutos() { return this.http.get<Produto[]>(this.estoqueUrl); }
  criarProduto(produto: Omit<Produto, 'id'>) { return this.http.post<Produto>(this.estoqueUrl, produto); }
  listarNotas() { return this.http.get<NotaFiscal[]>(this.faturamentoUrl); }
  criarNota(itens: ItemNotaFiscal[]) { return this.http.post<NotaFiscal>(this.faturamentoUrl, { itens }); }
  imprimirNota(id: string) { return this.http.post<NotaFiscal>(`${this.faturamentoUrl}/${id}/imprimir`, {}); }
}
