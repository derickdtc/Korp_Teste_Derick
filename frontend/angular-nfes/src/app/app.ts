import { isPlatformBrowser } from '@angular/common';
import { Component, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, forkJoin, of } from 'rxjs';
import { ApiError, ItemNotaFiscal, NotaFiscal, Produto } from './models';
import { NfeApiService } from './nfe-api.service';

type Tela = 'painel' | 'produtos' | 'notas';

@Component({ selector: 'app-root', imports: [FormsModule], templateUrl: './app.html', styleUrl: './app.css' })
export class App implements OnInit {
  private readonly api = inject(NfeApiService);
  private readonly platformId = inject(PLATFORM_ID);

  readonly telaAtual = signal<Tela>('painel');
  readonly produtos = signal<Produto[]>([]);
  readonly notas = signal<NotaFiscal[]>([]);
  readonly carregando = signal(true);
  readonly salvandoProduto = signal(false);
  readonly salvandoNota = signal(false);
  readonly imprimindoId = signal<string | null>(null);
  readonly mensagem = signal<{ tipo: 'sucesso' | 'erro'; texto: string } | null>(null);

  novoProduto = { codigo: '', descricao: '', saldo: 0 };
  itensDaNota: ItemNotaFiscal[] = [{ produtoId: '', quantidade: 1 }];

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) this.carregarDados();
  }

  selecionarTela(tela: Tela): void { this.telaAtual.set(tela); this.mensagem.set(null); }

  carregarDados(): void {
    this.carregando.set(true);
    let houveFalha = false;
    forkJoin({
      produtos: this.api.listarProdutos().pipe(catchError(() => {
        houveFalha = true;
        return of<Produto[]>([]);
      })),
      notas: this.api.listarNotas().pipe(catchError(() => {
        houveFalha = true;
        return of<NotaFiscal[]>([]);
      }))
    }).subscribe(({ produtos, notas }) => {
      this.produtos.set(produtos);
      this.notas.set(notas);
      this.carregando.set(false);
      if (houveFalha) {
        this.exibirErro('Não foi possível carregar um ou mais microsserviços. Verifique se estoque e faturamento estão em execução.');
      }
    });
  }

  salvarProduto(): void {
    if (!this.novoProduto.codigo.trim() || !this.novoProduto.descricao.trim() || this.novoProduto.saldo < 0) {
      this.exibirErro('Preencha código, descrição e um saldo válido.');
      return;
    }
    this.salvandoProduto.set(true);
    this.api.criarProduto({ codigo: this.novoProduto.codigo.trim(), descricao: this.novoProduto.descricao.trim(), saldo: Number(this.novoProduto.saldo) }).subscribe({
      next: () => {
        this.novoProduto = { codigo: '', descricao: '', saldo: 0 };
        this.salvandoProduto.set(false);
        this.exibirSucesso('Produto cadastrado com sucesso.');
        this.carregarDados();
      },
      error: erro => { this.salvandoProduto.set(false); this.exibirErro(this.mensagemDaApi(erro)); }
    });
  }

  adicionarItem(): void { this.itensDaNota.push({ produtoId: '', quantidade: 1 }); }
  removerItem(indice: number): void {
    if (this.itensDaNota.length === 1) this.itensDaNota[0] = { produtoId: '', quantidade: 1 };
    else this.itensDaNota.splice(indice, 1);
  }

  salvarNota(): void {
    const itens = this.itensDaNota.map(item => ({ produtoId: item.produtoId, quantidade: Number(item.quantidade) }));
    if (itens.some(item => !item.produtoId || item.quantidade <= 0)) {
      this.exibirErro('Selecione um produto e informe uma quantidade maior que zero em todos os itens.');
      return;
    }
    this.salvandoNota.set(true);
    this.api.criarNota(itens).subscribe({
      next: nota => {
        this.itensDaNota = [{ produtoId: '', quantidade: 1 }];
        this.salvandoNota.set(false);
        this.exibirSucesso(`Nota fiscal nº ${nota.numero} criada como aberta.`);
        this.carregarDados();
        this.telaAtual.set('notas');
      },
      error: erro => { this.salvandoNota.set(false); this.exibirErro(this.mensagemDaApi(erro)); }
    });
  }

  imprimirNota(nota: NotaFiscal): void {
    if (nota.status !== 'Aberta') return;
    this.imprimindoId.set(nota.id);
    this.api.imprimirNota(nota.id).subscribe({
      next: notaAtualizada => {
        this.imprimindoId.set(null);
        this.notas.update(notas => notas.map(item => item.id === notaAtualizada.id ? notaAtualizada : item));
        this.exibirSucesso(`Nota fiscal nº ${notaAtualizada.numero} fechada e enviada para impressão.`);
        window.setTimeout(() => window.print(), 250);
      },
      error: erro => { this.imprimindoId.set(null); this.exibirErro(this.mensagemDaApi(erro)); }
    });
  }

  totalItens(nota: NotaFiscal): number { return nota.itens.reduce((total, item) => total + item.quantidade, 0); }
  produtosComSaldo(): number { return this.produtos().filter(produto => produto.saldo > 0).length; }
  saldoTotal(): number { return this.produtos().reduce((total, produto) => total + produto.saldo, 0); }
  notasAbertas(): number { return this.notas().filter(nota => nota.status === 'Aberta').length; }
  notasFechadas(): number { return this.notas().filter(nota => nota.status === 'Fechada').length; }
  produtoPorId(id: string): Produto | undefined { return this.produtos().find(produto => produto.id === id); }
  exibirSucesso(texto: string): void { this.mensagem.set({ tipo: 'sucesso', texto }); }
  exibirErro(texto: string): void { this.mensagem.set({ tipo: 'erro', texto }); }
  private mensagemDaApi(erro: { error?: ApiError }): string { return erro?.error?.message ?? 'Não foi possível concluir a operação. Tente novamente.'; }
}
